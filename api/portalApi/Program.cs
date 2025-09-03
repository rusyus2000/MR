using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using portalApi.Middleware;
using SutterAnalyticsApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Windows Authentication (IIS handles it natively, so skip AddNegotiate)
builder.Services.AddAuthentication(); // No AddNegotiate on IIS

builder.Services.AddAuthorization();

// CORS configuration based on environment
string[] devOrigins = { "http://localhost:5174" };
string[] prodOrigins = { "http://smf-appweb-dev", "http://smf-appweb-dev.sutterhealth.org", "https://smf-appweb-dev.sutterhealth.org" }; // Add all production origins you expect

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins(devOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(prodOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

var app = builder.Build();

app.UseRouting();

// Move UseCors early
app.UseCors();

// Apply auth middleware AFTER CORS
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<EnsureUserExistsMiddleware>();

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
        db.SaveChanges();
    }
    catch { }
}

app.MapControllers();
app.Run();
