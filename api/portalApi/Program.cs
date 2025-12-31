using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using portalApi.Middleware;
using SutterAnalyticsApi.Data;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Windows Authentication (IIS handles it natively, so skip AddNegotiate)
builder.Services.AddAuthentication(); // No AddNegotiate on IIS

builder.Services.AddAuthorization();
// Shared in-proc cache for user lookups
builder.Services.AddMemoryCache();
builder.Services.Configure<SutterAnalyticsApi.Options.UserCacheOptions>(
    builder.Configuration.GetSection("UserCache"));
builder.Services.Configure<SutterAnalyticsApi.Options.AdminOptions>(
    builder.Configuration.GetSection("Admin"));

// CORS configuration based on environment
string[] devOrigins = { "http://localhost:5174" };
string[] prodOrigins = { "http://smf-appweb-dev", "http://smf-appweb-dev.sutterhealth.org", "https://smf-appweb-dev.sutterhealth.org", "http://smf-appweb-qa.sutterhealth.org", "http://smf-appweb-qa" }; // Add all production origins you expect
var allowedOrigins = devOrigins.Concat(prodOrigins).ToArray();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
    );
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// HttpClient for Search API (reused, pooled)
builder.Services.AddHttpClient("SearchApi", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["SearchApiUrl"] ?? "http://smf-appweb-qa/AI_search/";
    // Ensure trailing slash so relative requests like "rebuild-index" resolve under /AI_search/
    if (!baseUrl.EndsWith("/")) baseUrl += "/";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // Some Search API endpoints (e.g. rebuild-index) may require Windows auth.
    // In dev, set SearchApi:UseDefaultCredentials=true to send the process identity.
    UseDefaultCredentials = builder.Configuration.GetValue<bool?>("SearchApi:UseDefaultCredentials") ?? false,
    Credentials = (builder.Configuration.GetValue<bool?>("SearchApi:UseDefaultCredentials") ?? false)
        ? CredentialCache.DefaultNetworkCredentials
        : null
});

var app = builder.Build();

app.UseRouting();

// Move UseCors early
app.UseCors();

// Handle CORS preflight for PUT/DELETE under Windows Auth (adds CORS headers + skips auth)
app.Use(async (context, next) =>
{
    if (HttpMethods.Options.Equals(context.Request.Method, StringComparison.OrdinalIgnoreCase))
    {
        var origin = context.Request.Headers["Origin"].ToString();
        if (!string.IsNullOrEmpty(origin))
        {
            var allowed = allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
            if (allowed)
            {
                context.Response.Headers["Access-Control-Allow-Origin"] = origin;
                context.Response.Headers["Vary"] = "Origin";
                context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
                context.Response.Headers["Access-Control-Allow-Headers"] = context.Request.Headers["Access-Control-Request-Headers"].ToString();
                context.Response.Headers["Access-Control-Allow-Methods"] = "GET,POST,PUT,DELETE,OPTIONS";
            }
        }
        context.Response.StatusCode = StatusCodes.Status204NoContent;
        return;
    }
    await next();
});

// Apply auth middleware AFTER CORS
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<EnsureUserExistsMiddleware>();

// Warm up the Search API on startup (best-effort).
app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        try
        {
            var httpFactory = app.Services.GetRequiredService<IHttpClientFactory>();
            var client = httpFactory.CreateClient("SearchApi");
            await client.GetAsync("stats");
        }
        catch { }
    });
});

// Dev convenience: ensure Status lookup values exist
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var statuses = new[] { "Published", "Offline" };
        foreach (var s in statuses)
        {
            var exists = db.LookupValues.Any(l => l.Type == "Status" && l.Value == s);
            if (!exists)
            {
                db.LookupValues.Add(new SutterAnalyticsApi.Models.LookupValue { Type = "Status", Value = s });
            }
        }
        // Ensure a generic "Missing Data" option exists for common lookup types
        var typesNeedingMissing = new[]
        {
            "AssetType","Domain","Division","ServiceLine","DataSource","Status",
            "OperatingEntity","RefreshFrequency","DataConsumer"
        };
        foreach (var t in typesNeedingMissing)
        {
            var exists = db.LookupValues.Any(l => l.Type == t && l.Value == "Missing Data");
            if (!exists)
            {
                db.LookupValues.Add(new SutterAnalyticsApi.Models.LookupValue { Type = t, Value = "Missing Data" });
            }
        }
        db.SaveChanges();

        // Assume Missing Data employee (Id=11) exists per ops; do not insert here.

        // Seed sample data (only if none exists)
        if (!db.Items.Any())
        {
            var seeder = new SampleDataSeeder(db);
            seeder.Seed(100);
        }
    }
    catch { }
}

app.MapControllers();
app.Run();
