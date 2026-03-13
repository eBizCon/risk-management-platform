using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using CustomerManagement.Api.Models;

namespace CustomerManagement.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddOidcAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var oidcOptions = BuildOidcOptions(configuration);

        services.AddSingleton(Options.Create(oidcOptions));

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services.AddSingleton<IAuthorizationMiddlewareResultHandler, JsonAuthorizationResultHandler>();

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "session";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = environment.IsDevelopment()
                    ? CookieSecurePolicy.None
                    : CookieSecurePolicy.Always;
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.SlidingExpiration = true;
            })
            .AddOpenIdConnect(options =>
            {
                options.Authority = oidcOptions.Issuer;
                options.ClientId = oidcOptions.ClientId;
                options.ClientSecret = oidcOptions.ClientSecret;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.UsePkce = true;
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.CallbackPath = oidcOptions.CallbackPath;
                options.SignedOutCallbackPath = "/signout-callback-oidc";

                options.Scope.Clear();
                foreach (var scope in oidcOptions.Scope.Trim('"').Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    options.Scope.Add(scope);

                if (environment.IsDevelopment()) options.RequireHttpsMetadata = false;

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
                                claims,
                                oidcOptions.RolesClaimPath,
                                AppRoles.All);

                            var identity = context.Principal?.Identity as ClaimsIdentity;
                            if (identity != null)
                            {
                                foreach (var role in roles) identity.AddClaim(new Claim(ClaimTypes.Role, role));

                                if (!identity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
                                {
                                    var sub = identity.FindFirst("sub")?.Value;
                                    if (sub != null) identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, sub));
                                }

                                if (!identity.HasClaim(c => c.Type == ClaimTypes.Email))
                                {
                                    var email = identity.FindFirst("email")?.Value;
                                    if (email != null) identity.AddClaim(new Claim(ClaimTypes.Email, email));
                                }

                                if (!identity.HasClaim(c => c.Type == ClaimTypes.Name))
                                {
                                    var name = identity.FindFirst("name")?.Value
                                               ?? identity.FindFirst("preferred_username")?.Value;
                                    if (name != null) identity.AddClaim(new Claim(ClaimTypes.Name, name));
                                }
                            }
                        }
                        catch
                        {
                        }

                        return Task.CompletedTask;
                    },
                    OnRedirectToIdentityProviderForSignOut = context =>
                    {
                        context.ProtocolMessage.PostLogoutRedirectUri = oidcOptions.PostLogoutRedirectUri;
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.Applicant, policy => policy.RequireRole(AppRoles.Applicant));
            options.AddPolicy(AuthPolicies.Processor, policy => policy.RequireRole(AppRoles.Processor));
            options.AddPolicy(
                AuthPolicies.ApplicantOrProcessor,
                policy => policy.RequireRole(AppRoles.Applicant, AppRoles.Processor));
        });

        return services;
    }

    private static OidcOptions BuildOidcOptions(IConfiguration configuration)
    {
        var options = configuration.GetSection("Authentication").GetSection("Oidc").Get<OidcOptions>();
        ArgumentNullException.ThrowIfNull(options);
        return options;
    }
}
