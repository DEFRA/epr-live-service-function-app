using EPR.LiveService.FunctionApp.Extentions;
using System.Security.Claims;

namespace EPR.LiveService.FunctionApp.Authorisation;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class AuthorizeFunctionAttribute(params string[] roles) : Attribute
{
    public IReadOnlyCollection<string> Roles { get; } = Array.AsReadOnly(roles);

    public bool IsAuthorized(ClaimsPrincipal principal)
    {
        return principal.Identity?.IsAuthenticated == true
            && string.Equals(
                principal.GetTenantId(),
                AuthorisationConstants.ExpectedTenantId,
                StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(principal.GetObjectId())
            && Roles.Count > 0
            && Roles.Any(principal.HasRole);
    }
}
