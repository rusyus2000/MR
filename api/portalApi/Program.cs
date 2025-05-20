using Microsoft.EntityFrameworkCore;
using portalApi.Middleware;
using SutterAnalyticsApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Windows Authentication (IIS handles it natively, so skip AddNegotiate)
builder.Services.AddAuthentication(); // No AddNegotiate on IIS

builder.Services.AddAuthorization();

// ? Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:5174")
            .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

var app = builder.Build();

app.UseRouting();

// ? Move UseCors early and remove manual OPTIONS block
app.UseCors();

// Apply auth middleware AFTER CORS
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<EnsureUserExistsMiddleware>();

app.MapControllers();
app.Run();
