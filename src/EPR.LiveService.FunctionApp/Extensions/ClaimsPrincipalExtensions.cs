using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace EPR.LiveService.FunctionApp.Extensions;

[ExcludeFromCodeCoverage]
public static class ClaimsPrincipalExtensions
{
    public static string? GetTenantId(
        this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue("tid")
            ?? principal.FindFirstValue(
                "http://schemas.microsoft.com/identity/claims/tenantid");
    }

    public static string? GetObjectId(
        this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue("oid")
            ?? principal.FindFirstValue(
                "http://schemas.microsoft.com/identity/claims/objectidentifier");
    }

    public static string? GetDisplayName(
        this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue("name")
            ?? principal.FindFirstValue(ClaimTypes.Name);
    }

    public static string? GetEmail(
        this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue("preferred_username")
            ?? principal.FindFirstValue(ClaimTypes.Email)
            ?? principal.FindFirstValue(
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
    }

    public static bool HasRole(
        this ClaimsPrincipal principal,
        string requiredRole)
    {
        return principal.Claims.Any(claim =>
            IsRoleClaim(claim.Type) &&
            string.Equals(
                claim.Value,
                requiredRole,
                StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsRoleClaim(string claimType)
    {
        return string.Equals(
                   claimType,
                   "roles",
                   StringComparison.OrdinalIgnoreCase)
            || string.Equals(
                   claimType,
                   "role",
                   StringComparison.OrdinalIgnoreCase)
            || string.Equals(
                   claimType,
                   ClaimTypes.Role,
                   StringComparison.OrdinalIgnoreCase);
    }
}
