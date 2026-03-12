using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RiskManagement.Api.Services;

public class OidcConfiguration
{
    public string AuthorizationEndpoint { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = string.Empty;
    public string EndSessionEndpoint { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
}

public class AuthorizationRequest
{
    public string Url { get; set; } = string.Empty;
    public string CodeVerifier { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}

public class TokenExchangeResult
{
    public string IdToken { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public JsonElement? Claims { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class OidcService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private OidcConfiguration? _cachedConfig;
    private static readonly string[] AllowedRoles = { "applicant", "processor" };

    public OidcService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    private string GetEnvOrConfig(string key)
    {
        return Environment.GetEnvironmentVariable(key) ?? _configuration[key] ?? string.Empty;
    }

    public async Task<OidcConfiguration> GetOidcConfigurationAsync()
    {
        if (_cachedConfig != null) return _cachedConfig;

        var issuer = GetEnvOrConfig("OIDC_ISSUER");
        var client = _httpClientFactory.CreateClient();

        var handler = new HttpClientHandler();
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }

        using var devClient = new HttpClient(handler);
        var discoveryUrl = $"{issuer.TrimEnd('/')}/.well-known/openid-configuration";
        var response = await devClient.GetAsync(discoveryUrl);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        _cachedConfig = new OidcConfiguration
        {
            AuthorizationEndpoint = json.GetProperty("authorization_endpoint").GetString() ?? string.Empty,
            TokenEndpoint = json.GetProperty("token_endpoint").GetString() ?? string.Empty,
            EndSessionEndpoint = json.TryGetProperty("end_session_endpoint", out var endSession)
                ? endSession.GetString() ?? string.Empty
                : string.Empty,
            Issuer = issuer
        };

        return _cachedConfig;
    }

    public async Task<AuthorizationRequest> CreateAuthorizationRequestAsync()
    {
        var config = await GetOidcConfigurationAsync();
        var redirectUri = GetEnvOrConfig("OIDC_REDIRECT_URI");
        var clientId = GetEnvOrConfig("OIDC_CLIENT_ID");
        var scope = GetEnvOrConfig("OIDC_SCOPE");

        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        var state = GenerateState();

        var url = $"{config.AuthorizationEndpoint}?" +
                  $"response_type=code&" +
                  $"client_id={Uri.EscapeDataString(clientId)}&" +
                  $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                  $"scope={Uri.EscapeDataString(scope)}&" +
                  $"code_challenge={Uri.EscapeDataString(codeChallenge)}&" +
                  $"code_challenge_method=S256&" +
                  $"state={Uri.EscapeDataString(state)}";

        return new AuthorizationRequest
        {
            Url = url,
            CodeVerifier = codeVerifier,
            State = state
        };
    }

    public async Task<TokenExchangeResult> ExchangeAuthorizationCodeAsync(
        string code, string codeVerifier)
    {
        var config = await GetOidcConfigurationAsync();
        var clientId = GetEnvOrConfig("OIDC_CLIENT_ID");
        var clientSecret = GetEnvOrConfig("OIDC_CLIENT_SECRET");
        var redirectUri = GetEnvOrConfig("OIDC_REDIRECT_URI");
        var rolesClaimPath = GetEnvOrConfig("OIDC_ROLES_CLAIM_PATH");

        var handler = new HttpClientHandler();
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }

        using var client = new HttpClient(handler);

        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri,
            ["client_id"] = clientId,
            ["code_verifier"] = codeVerifier
        };

        if (!string.IsNullOrEmpty(clientSecret))
        {
            parameters["client_secret"] = clientSecret;
        }

        var response = await client.PostAsync(config.TokenEndpoint, new FormUrlEncodedContent(parameters));
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<JsonElement>();

        var idToken = tokenResponse.GetProperty("id_token").GetString()
            ?? throw new InvalidOperationException("Missing id_token in token response");
        var accessToken = tokenResponse.GetProperty("access_token").GetString()
            ?? throw new InvalidOperationException("Missing access_token in token response");

        var accessClaims = DecodeJwtPayload(accessToken);
        var roles = ExtractRolesFromClaims(accessClaims, rolesClaimPath, AllowedRoles);

        JsonElement? idClaims = null;
        try { idClaims = DecodeJwtPayload(idToken); } catch { }

        return new TokenExchangeResult
        {
            IdToken = idToken,
            AccessToken = accessToken,
            Claims = idClaims,
            Roles = roles
        };
    }

    public async Task<string?> BuildLogoutUrlAsync(string idToken)
    {
        var config = await GetOidcConfigurationAsync();
        var postLogoutRedirectUri = GetEnvOrConfig("OIDC_POST_LOGOUT_REDIRECT_URI");

        if (string.IsNullOrEmpty(config.EndSessionEndpoint))
        {
            return null;
        }

        return $"{config.EndSessionEndpoint}?" +
               $"post_logout_redirect_uri={Uri.EscapeDataString(postLogoutRedirectUri)}&" +
               $"id_token_hint={Uri.EscapeDataString(idToken)}";
    }

    public static JsonElement DecodeJwtPayload(string token)
    {
        var parts = token.Split('.');
        if (parts.Length < 2)
        {
            throw new InvalidOperationException("Invalid JWT format for access_token");
        }

        var payload = parts[1];
        var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var normalized = padded.Replace('-', '+').Replace('_', '/');
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(normalized));
        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    public static List<string> ExtractRolesFromClaims(
        JsonElement claims, string claimPath, string[] allowedRoles)
    {
        if (string.IsNullOrEmpty(claimPath))
        {
            return new List<string>();
        }

        var parts = claimPath.Split('.');
        var current = claims;

        foreach (var part in parts)
        {
            if (current.ValueKind != JsonValueKind.Object)
            {
                return new List<string>();
            }

            if (!current.TryGetProperty(part, out var next))
            {
                return new List<string>();
            }

            current = next;
        }

        var roles = new List<string>();

        if (current.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in current.EnumerateArray())
            {
                if (element.ValueKind == JsonValueKind.String)
                {
                    var role = element.GetString();
                    if (role != null && allowedRoles.Contains(role))
                    {
                        roles.Add(role);
                    }
                }
            }
        }
        else if (current.ValueKind == JsonValueKind.String)
        {
            var role = current.GetString();
            if (role != null && allowedRoles.Contains(role))
            {
                roles.Add(role);
            }
        }

        return roles;
    }

    public void ResetConfigurationCache()
    {
        _cachedConfig = null;
    }

    private static string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Base64UrlEncode(bytes);
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
        return Base64UrlEncode(hash);
    }

    private static string GenerateState()
    {
        var bytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
