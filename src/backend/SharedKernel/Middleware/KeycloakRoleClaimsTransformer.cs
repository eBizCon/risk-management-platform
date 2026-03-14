using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace SharedKernel.Middleware;

public sealed class KeycloakRoleClaimsTransformer : IClaimsTransformation
{
    private readonly string _rolesClaimPath;

    public KeycloakRoleClaimsTransformer(string rolesClaimPath)
    {
        _rolesClaimPath = rolesClaimPath;
    }

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
            return Task.FromResult(principal);

        if (identity.HasClaim(c => c.Type == ClaimTypes.Role))
            return Task.FromResult(principal);

        var roles = ExtractRolesFromClaims(identity, _rolesClaimPath);
        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        var sub = identity.FindFirst("sub")?.Value;
        if (sub is not null && !identity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
        {
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, sub));
        }

        var email = identity.FindFirst("email")?.Value;
        if (email is not null && !identity.HasClaim(c => c.Type == ClaimTypes.Email))
        {
            identity.AddClaim(new Claim(ClaimTypes.Email, email));
        }

        var name = identity.FindFirst("name")?.Value
                   ?? identity.FindFirst("preferred_username")?.Value;
        if (name is not null && !identity.HasClaim(c => c.Type == ClaimTypes.Name))
        {
            identity.AddClaim(new Claim(ClaimTypes.Name, name));
        }

        return Task.FromResult(principal);
    }

    private static IEnumerable<string> ExtractRolesFromClaims(ClaimsIdentity identity, string claimPath)
    {
        if (string.IsNullOrWhiteSpace(claimPath))
            return [];

        var pathParts = claimPath.Split('.');
        var rootClaimType = pathParts[0];
        var rootClaim = identity.FindFirst(rootClaimType);

        if (rootClaim is null)
            return [];

        if (pathParts.Length == 1)
        {
            return rootClaim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(rootClaim.Value);
            return NavigateJsonPath(json, pathParts.AsSpan(1));
        }
        catch
        {
            return [];
        }
    }

    private static IEnumerable<string> NavigateJsonPath(JsonElement current, ReadOnlySpan<string> remainingPath)
    {
        foreach (var part in remainingPath)
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(part, out var next))
                return [];
            current = next;
        }

        if (current.ValueKind == JsonValueKind.Array)
        {
            return current.EnumerateArray()
                .Where(e => e.ValueKind == JsonValueKind.String)
                .Select(e => e.GetString()!)
                .Where(s => !string.IsNullOrWhiteSpace(s));
        }

        if (current.ValueKind == JsonValueKind.String)
        {
            var value = current.GetString();
            return string.IsNullOrWhiteSpace(value) ? [] : [value];
        }

        return [];
    }
}
