namespace RiskManagement.Api.Extensions;

public class OidcOptions
{
    public string Issuer { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string Scope { get; set; } = "openid profile email";
    public string RolesClaimPath { get; set; } = "";
    public string PostLogoutRedirectUri { get; set; } = "/";
    public string RedirectUri { get; set; } = "/";
    public string CallbackPath { get; set; } = "/auth/callback";
}
