using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using RiskManagement.Api.Extensions;
using RiskManagement.Api.Middleware;
using RiskManagement.Api.Models;
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

var oidcIssuer = builder.Configuration["OIDC_ISSUER"] ?? "http://localhost:8081/realms/risk-management";
var oidcRolesClaimPath = builder.Configuration["OIDC_ROLES_CLAIM_PATH"] ?? "realm_access.roles";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = oidcIssuer;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = oidcIssuer,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

builder.Services.AddTransient<IClaimsTransformation>(
    _ => new KeycloakRoleClaimsTransformer(oidcRolesClaimPath));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.Applicant, policy => policy.RequireRole(AppRoles.Applicant));
    options.AddPolicy(AuthPolicies.Processor, policy => policy.RequireRole(AppRoles.Processor));
    options.AddPolicy(
        AuthPolicies.ApplicantOrProcessor,
        policy => policy.RequireRole(AppRoles.Applicant, AppRoles.Processor));
    options.AddPolicy(AuthPolicies.RiskManager, policy => policy.RequireRole(AppRoles.RiskManager));
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
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseMiddleware<InternalAuthMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program accessible for integration tests
public partial class Program
{
}