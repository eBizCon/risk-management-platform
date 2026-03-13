using System.Text.Json;
using RiskManagement.Api.Services;

namespace RiskManagement.Api.Tests;

public class OidcServiceTests
{
    private static readonly string[] AllowedRoles = { "applicant", "processor" };

    [Fact]
    public void ExtractRolesFromClaims_Returns_Matching_Roles_From_Array_Path()
    {
        var json = JsonSerializer.Deserialize<JsonElement>(
            """{"resource_access": {"roles": ["applicant", "other", "processor"]}}""");

        var roles = OidcService.ExtractRolesFromClaims(json, "resource_access.roles", AllowedRoles);

        Assert.Equal(new[] { "applicant", "processor" }, roles);
    }

    [Fact]
    public void ExtractRolesFromClaims_Returns_Single_Role_When_String()
    {
        var json = JsonSerializer.Deserialize<JsonElement>(
            """{"resource_access": {"role": "processor"}}""");

        var roles = OidcService.ExtractRolesFromClaims(json, "resource_access.role", AllowedRoles);

        Assert.Equal(new[] { "processor" }, roles);
    }

    [Fact]
    public void ExtractRolesFromClaims_Returns_Empty_When_Path_Missing()
    {
        var json = JsonSerializer.Deserialize<JsonElement>(
            """{"other": {}}""");

        var roles = OidcService.ExtractRolesFromClaims(json, "resource_access.roles", AllowedRoles);

        Assert.Empty(roles);
    }

    [Fact]
    public void ExtractRolesFromClaims_Returns_Empty_For_Empty_ClaimPath()
    {
        var json = JsonSerializer.Deserialize<JsonElement>(
            """{"resource_access": {"roles": ["applicant"]}}""");

        var roles = OidcService.ExtractRolesFromClaims(json, "", AllowedRoles);

        Assert.Empty(roles);
    }
}
