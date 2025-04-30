using Microsoft.EntityFrameworkCore;
using SutterAnalyticsApi.Data;

var builder = WebApplication.CreateBuilder(args);

// 1) EF Core InMemory
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseInMemoryDatabase("SutterAnalyticsDb"));

// 2) Seeder
builder.Services.AddTransient<SampleDataSeeder>();

// 3) Controllers
builder.Services.AddControllers();

var app = builder.Build();

// 4) Seed data
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<SampleDataSeeder>();
    seeder.Seed();
}

app.MapControllers();
app.Run();
