using System.Text.Json;
using RiskManagement.Api.Extensions;

namespace RiskManagement.Api.Tests;

public class RoleClaimHelperTests
{
    private static readonly string[] AllowedRoles = { "applicant", "processor" };

    [Fact]
    public void ExtractRolesFromClaims_Returns_Matching_Roles_From_Array_Path()
    {
        var json = JsonSerializer.Deserialize<JsonElement>(
            """{"resource_access": {"roles": ["applicant", "other", "processor"]}}""");

        var roles = RoleClaimHelper.ExtractRolesFromClaims(json, "resource_access.roles", AllowedRoles);

        Assert.Equal(new[] { "applicant", "processor" }, roles);
    }

    [Fact]
    public void ExtractRolesFromClaims_Returns_Single_Role_When_String()
    {
        var json = JsonSerializer.Deserialize<JsonElement>(
            """{"resource_access": {"role": "processor"}}""");

        var roles = RoleClaimHelper.ExtractRolesFromClaims(json, "resource_access.role", AllowedRoles);

        Assert.Equal(new[] { "processor" }, roles);
    }

    [Fact]
    public void ExtractRolesFromClaims_Returns_Empty_When_Path_Missing()
    {
        var json = JsonSerializer.Deserialize<JsonElement>(
            """{"other": {}}""");

        var roles = RoleClaimHelper.ExtractRolesFromClaims(json, "resource_access.roles", AllowedRoles);

        Assert.Empty(roles);
    }

    [Fact]
    public void ExtractRolesFromClaims_Returns_Empty_For_Empty_ClaimPath()
    {
        var json = JsonSerializer.Deserialize<JsonElement>(
            """{"resource_access": {"roles": ["applicant"]}}""");

        var roles = RoleClaimHelper.ExtractRolesFromClaims(json, "", AllowedRoles);

        Assert.Empty(roles);
    }
}
