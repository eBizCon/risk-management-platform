using System.Security.Claims;
using CustomerManagement.Api.Models;

namespace CustomerManagement.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Email) ?? "";
    }

    public static bool IsApplicant(this ClaimsPrincipal principal)
    {
        return principal.HasClaim(ClaimTypes.Role, AppRoles.Applicant);
    }
}