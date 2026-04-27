using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using RiskManagement.Api.Extensions;
using RiskManagement.Api.Middleware;
using RiskManagement.Api.Models;
using RiskManagement.Infrastructure;
using RiskManagement.Infrastructure.Persistence;
using RiskManagement.Infrastructure.Services;
using SharedKernel.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

var customerServiceUrl = builder.Configuration["CUSTOMER_SERVICE_URL"] ?? "http://localhost:5000";
var serviceApiKey = builder.Configuration["SERVICE_API_KEY"] ?? "";

var rabbitMqConnectionString = builder.Configuration.GetConnectionString("messaging")
                               ?? builder.Configuration["RabbitMQ:ConnectionString"]
                               ?? "amqp://guest:guest@localhost:5672";

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplicationServices(customerServiceUrl, serviceApiKey);
builder.Services.AddMessaging(rabbitMqConnectionString);

builder.Services.AddHttpClient("CustomerSyncClient", client =>
{
    client.BaseAddress = new Uri(customerServiceUrl);
    client.DefaultRequestHeaders.Add("X-Api-Key", serviceApiKey);
});
builder.Services.AddHostedService<CustomerReadModelSyncService>();

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

builder.Services.AddTransient<IClaimsTransformation>(_ => new KeycloakRoleClaimsTransformer(oidcRolesClaimPath));

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

// Database migration
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Middleware pipeline
app.UseMiddleware<RiskManagement.Api.Middleware.ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseMiddleware<InternalAuthMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.MapDefaultEndpoints();

app.Run();

// Make Program accessible for integration tests
public partial class Program
{
}