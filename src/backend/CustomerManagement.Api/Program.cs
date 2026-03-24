using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using CustomerManagement.Api.Models;
using CustomerManagement.Infrastructure;
using CustomerManagement.Infrastructure.Persistence;
using SharedKernel.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var applicationServiceUrl = builder.Configuration["APPLICATION_SERVICE_URL"] ?? "http://localhost:5227";
var serviceApiKey = builder.Configuration["SERVICE_API_KEY"] ?? "";

var rabbitMqConnectionString = builder.Configuration.GetConnectionString("messaging")
                               ?? builder.Configuration["RabbitMQ:ConnectionString"]
                               ?? "amqp://risk:risk@localhost:5672";

builder.Services.AddCustomerInfrastructure(connectionString);
builder.Services.AddCustomerApplicationServices(applicationServiceUrl, serviceApiKey);
builder.Services.AddMessaging(rabbitMqConnectionString);

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
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseAuthentication();
app.UseMiddleware<InternalAuthMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.MapDefaultEndpoints();

app.Run();

public partial class Program
{
}