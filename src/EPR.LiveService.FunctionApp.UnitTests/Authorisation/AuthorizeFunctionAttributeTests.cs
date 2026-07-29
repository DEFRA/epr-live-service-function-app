using EPR.LiveService.FunctionApp.Authorisation;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Claims;

namespace EPR.LiveService.FunctionApp.UnitTests.Authorisation;

[TestClass]
public sealed class AuthorizeFunctionAttributeTests
{
    private readonly AuthorizeFunctionAttribute _attribute =
        new(Roles.User, Roles.Admin);

    [DataRow("user")]
    [DataRow("ADMIN")]
    [TestMethod]
    public void IsAuthorized_WhenUserHasAnyRequiredRole_ReturnsTrue(string role)
    {
        var principal = CreatePrincipal(new Claim("roles", role));

        _attribute.IsAuthorized(principal).Should().BeTrue();
    }

    [TestMethod]
    public void IsAuthorized_WhenUserDoesNotHaveARequiredRole_ReturnsFalse()
    {
        var principal = CreatePrincipal(new Claim("roles", "reader"));

        _attribute.IsAuthorized(principal).Should().BeFalse();
    }

    [TestMethod]
    public void IsAuthorized_WhenUserBelongsToAnotherTenant_ReturnsFalse()
    {
        var principal = CreatePrincipal(
            new Claim("roles", Roles.User),
            new Claim("tid", "another-tenant"));

        _attribute.IsAuthorized(principal).Should().BeFalse();
    }

    [TestMethod]
    public void IsAuthorized_WhenUserHasNoObjectId_ReturnsFalse()
    {
        var principal = CreatePrincipal(
            new Claim("roles", Roles.User),
            new Claim("oid", string.Empty));

        _attribute.IsAuthorized(principal).Should().BeFalse();
    }

    [TestMethod]
    public void IsAuthorized_WhenUserIsNotAuthenticated_ReturnsFalse()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim("roles", Roles.User)]));

        _attribute.IsAuthorized(principal).Should().BeFalse();
    }

    private static ClaimsPrincipal CreatePrincipal(params Claim[] claims)
    {
        var allClaims = claims.ToList();

        if (!allClaims.Any(claim => claim.Type == "tid"))
        {
            allClaims.Add(
                new Claim("tid", AuthorisationConstants.ExpectedTenantId));
        }

        if (!allClaims.Any(claim => claim.Type == "oid"))
        {
            allClaims.Add(new Claim("oid", "object-id"));
        }

        var identity = new ClaimsIdentity(allClaims, "EasyAuth");

        return new ClaimsPrincipal(identity);
    }
}
