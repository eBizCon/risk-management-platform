using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using RiskManagement.Api.Extensions;
using RiskManagement.Api.Middleware;
using RiskManagement.Infrastructure;
using RiskManagement.Infrastructure.Persistence;
using RiskManagement.Infrastructure.Seeding;
using SharedKernel.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

var customerServiceUrl = builder.Configuration["CUSTOMER_SERVICE_URL"] ?? "http://localhost:5001";
var serviceApiKey = builder.Configuration["SERVICE_API_KEY"] ?? "";

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplicationServices(customerServiceUrl, serviceApiKey);

var sharedKeysPath = Path.Combine(builder.Environment.ContentRootPath, "..", "shared-keys");
builder.Services.AddDataProtection()
    .SetApplicationName("risk-management-platform")
    .PersistKeysToFileSystem(new DirectoryInfo(sharedKeysPath));

builder.Services.AddOidcAuthentication(builder.Configuration, builder.Environment);

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
    await dbContext.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

// Middleware pipeline
if (app.Environment.IsDevelopment()) app.UseCors();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ApiKeyAuthMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

// SPA fallback - serve index.html for all non-API, non-file routes
app.MapFallbackToFile("index.html");

app.Run();

// Make Program accessible for integration tests
public partial class Program
{
}