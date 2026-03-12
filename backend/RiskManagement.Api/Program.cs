using Microsoft.EntityFrameworkCore;
using RiskManagement.Api.Data;
using RiskManagement.Api.Middleware;
using RiskManagement.Api.Services;
using RiskManagement.Api.Validation;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=risk_management;Username=risk;Password=risk";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSingleton<ScoringService>();
builder.Services.AddSingleton<SessionService>();
builder.Services.AddScoped<OidcService>();
builder.Services.AddScoped<ApplicationRepository>();
builder.Services.AddSingleton<ApplicationValidator>();
builder.Services.AddSingleton<ApplicationUpdateValidator>();
builder.Services.AddSingleton<ProcessorDecisionValidator>();
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:4173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Database migration and seeding
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    // Ensure application_inquiries table exists (EnsureCreatedAsync only works on empty databases)
    await dbContext.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS application_inquiries (
            id SERIAL PRIMARY KEY,
            application_id INTEGER NOT NULL REFERENCES applications(id) ON DELETE CASCADE,
            inquiry_text TEXT NOT NULL,
            status TEXT NOT NULL DEFAULT 'open',
            processor_email TEXT NOT NULL,
            response_text TEXT,
            created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            responded_at TIMESTAMPTZ
        )");

    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseCors();
}

app.UseMiddleware<AuthMiddleware>();
app.UseStaticFiles();
app.MapControllers();

// SPA fallback - serve index.html for all non-API, non-file routes
app.MapFallbackToFile("index.html");

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
