using System.Security.Claims;
using RiskManagement.Api.Models;

namespace RiskManagement.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
    }

    public static string GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Email) ?? "";
    }

    public static string GetName(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Name) ?? "Unbekannter Nutzer";
    }

    public static string GetRole(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Role) ?? "";
    }

    public static bool IsInRole(this ClaimsPrincipal principal, string role)
    {
        return principal.HasClaim(ClaimTypes.Role, role);
    }

    public static UserDto ToUserDto(this ClaimsPrincipal principal)
    {
        return new UserDto
        {
            Id = principal.GetUserId(),
            Email = principal.GetEmail(),
            Name = principal.GetName(),
            Role = principal.GetRole()
        };
    }
}
