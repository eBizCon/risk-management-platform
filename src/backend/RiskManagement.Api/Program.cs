using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using RiskManagement.Api.Data;
using RiskManagement.Api.Extensions;
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
builder.Services.AddScoped<ApplicationRepository>();
builder.Services.AddSingleton<ApplicationValidator>();
builder.Services.AddSingleton<ApplicationUpdateValidator>();
builder.Services.AddSingleton<ProcessorDecisionValidator>();
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddHttpClient();

var oidcIssuer = Environment.GetEnvironmentVariable("OIDC_ISSUER") ?? builder.Configuration["OIDC_ISSUER"] ?? "";
var oidcClientId = Environment.GetEnvironmentVariable("OIDC_CLIENT_ID") ?? builder.Configuration["OIDC_CLIENT_ID"] ?? "";
var oidcClientSecret = Environment.GetEnvironmentVariable("OIDC_CLIENT_SECRET") ?? builder.Configuration["OIDC_CLIENT_SECRET"] ?? "";
var oidcScope = Environment.GetEnvironmentVariable("OIDC_SCOPE") ?? builder.Configuration["OIDC_SCOPE"] ?? "openid profile email";
var oidcRolesClaimPath = Environment.GetEnvironmentVariable("OIDC_ROLES_CLAIM_PATH") ?? builder.Configuration["OIDC_ROLES_CLAIM_PATH"] ?? "";
var oidcPostLogoutRedirectUri = Environment.GetEnvironmentVariable("OIDC_POST_LOGOUT_REDIRECT_URI") ?? builder.Configuration["OIDC_POST_LOGOUT_REDIRECT_URI"] ?? "/";

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = "session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;

    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsJsonAsync(new { error = "Login erforderlich" });
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsJsonAsync(new { error = "Keine Berechtigung" });
        }
        context.Response.StatusCode = 403;
        return context.Response.WriteAsync("Keine Berechtigung");
    };
})
.AddOpenIdConnect(options =>
{
    options.Authority = oidcIssuer;
    options.ClientId = oidcClientId;
    options.ClientSecret = oidcClientSecret;
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.UsePkce = true;
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.CallbackPath = "/auth/callback";
    options.SignedOutCallbackPath = "/signout-callback-oidc";

    options.Scope.Clear();
    foreach (var scope in oidcScope.Trim('"').Split(' ', StringSplitOptions.RemoveEmptyEntries))
    {
        options.Scope.Add(scope);
    }

    if (builder.Environment.IsDevelopment())
    {
        options.RequireHttpsMetadata = false;
        options.BackchannelHttpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
    }

    options.Events = new OpenIdConnectEvents
    {
        OnTokenValidated = context =>
        {
            var accessToken = context.TokenEndpointResponse?.AccessToken;
            if (string.IsNullOrEmpty(accessToken)) return Task.CompletedTask;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(accessToken);
                var payload = jwt.Payload.SerializeToJson();
                var claims = JsonSerializer.Deserialize<JsonElement>(payload);

                var roles = RoleClaimHelper.ExtractRolesFromClaims(
                    claims, oidcRolesClaimPath, RoleClaimHelper.AllowedRoles);

                var identity = context.Principal?.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    foreach (var role in roles)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }

                    if (!identity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
                    {
                        var sub = identity.FindFirst("sub")?.Value;
                        if (sub != null)
                            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, sub));
                    }

                    if (!identity.HasClaim(c => c.Type == ClaimTypes.Email))
                    {
                        var email = identity.FindFirst("email")?.Value;
                        if (email != null)
                            identity.AddClaim(new Claim(ClaimTypes.Email, email));
                    }

                    if (!identity.HasClaim(c => c.Type == ClaimTypes.Name))
                    {
                        var name = identity.FindFirst("name")?.Value
                            ?? identity.FindFirst("preferred_username")?.Value;
                        if (name != null)
                            identity.AddClaim(new Claim(ClaimTypes.Name, name));
                    }
                }
            }
            catch
            {
                // Token parsing failed - roles will be empty
            }

            return Task.CompletedTask;
        },

        OnRedirectToIdentityProviderForSignOut = context =>
        {
            context.ProtocolMessage.PostLogoutRedirectUri = oidcPostLogoutRedirectUri;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

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
if (app.Environment.IsDevelopment())
{
    app.UseCors();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

// SPA fallback - serve index.html for all non-API, non-file routes
app.MapFallbackToFile("index.html");

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
