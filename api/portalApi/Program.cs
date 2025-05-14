using Microsoft.EntityFrameworkCore;
using portalApi.Middleware;
using SutterAnalyticsApi.Data;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("Windows")
    .AddNegotiate(); // <- uses Windows auth

builder.Services.AddAuthorization();

builder.Services.AddCors(opts => {
    opts.AddDefaultPolicy(policy => policy
      .WithOrigins("http://localhost:5174")
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials()
    );
});

// Use SQL Server instead of InMemory
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// 3) Controllers
builder.Services.AddControllers();


var app = builder.Build();


app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<EnsureUserExistsMiddleware>();
app.MapControllers();
app.Run();
