using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using CustomerManagement.Api.Extensions;
using CustomerManagement.Infrastructure;
using CustomerManagement.Infrastructure.Persistence;
using SharedKernel.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var applicationServiceUrl = builder.Configuration["APPLICATION_SERVICE_URL"] ?? "http://localhost:5000";
var serviceApiKey = builder.Configuration["SERVICE_API_KEY"] ?? "";

builder.Services.AddCustomerInfrastructure(connectionString);
builder.Services.AddCustomerApplicationServices(applicationServiceUrl, serviceApiKey);

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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment()) app.UseCors();

app.UseMiddleware<ApiKeyAuthMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

public partial class Program
{
}