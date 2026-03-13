using System.Text.Json;

namespace CustomerManagement.Api.Extensions;

public static class RoleClaimHelper
{
    public static List<string> ExtractRolesFromClaims(
        JsonElement claims, string claimPath, string[] allowedRoles)
    {
        if (string.IsNullOrEmpty(claimPath)) return new List<string>();

        var parts = claimPath.Split('.');
        var current = claims;

        foreach (var part in parts)
        {
            if (current.ValueKind != JsonValueKind.Object) return new List<string>();
            if (!current.TryGetProperty(part, out var next)) return new List<string>();
            current = next;
        }

        var roles = new List<string>();

        if (current.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in current.EnumerateArray())
                if (element.ValueKind == JsonValueKind.String)
                {
                    var role = element.GetString();
                    if (role != null && allowedRoles.Contains(role)) roles.Add(role);
                }
        }
        else if (current.ValueKind == JsonValueKind.String)
        {
            var role = current.GetString();
            if (role != null && allowedRoles.Contains(role)) roles.Add(role);
        }

        return roles;
    }
}
