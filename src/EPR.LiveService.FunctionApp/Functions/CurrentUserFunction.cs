using System;
using System.Collections.Generic;
using System.Text;

namespace EPR.LiveService.FunctionApp.Functions;

using EPR.LiveService.FunctionApp.Authorisation;
using EPR.LiveService.FunctionApp.Extentions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Security.Claims;

public sealed class CurrentUserFunction
{
    private const string ExpectedTenantId =
        "6f504113-6b64-43f2-ade9-242e05780007";

    [Function("CurrentUser")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "me")]
        HttpRequestData request)
    {
        var principal = EasyAuthPrincipal.Parse(request);

        if (principal?.Identity?.IsAuthenticated != true)
        {
            return request.CreateResponse(
                HttpStatusCode.Unauthorized);
        }

        var tenantId = principal.GetTenantId();
        var objectId = principal.GetObjectId();

        if (!string.Equals(
                tenantId,
                ExpectedTenantId,
                StringComparison.OrdinalIgnoreCase))
        {
            return request.CreateResponse(
                HttpStatusCode.Forbidden);
        }

        if (string.IsNullOrWhiteSpace(objectId))
        {
            return request.CreateResponse(
                HttpStatusCode.Forbidden);
        }

        // At this point, the user is authenticated.
        // authorisation is based on whether the given role claims
        // match the claims we hold in a db/entra for them.

        var dbUserRoles = new List<string>
        {
            "user",
            "admin"
        };

        if (dbUserRoles == null || !dbUserRoles.Any(role => principal.HasRole(role)))
        {
            return request.CreateResponse(
                HttpStatusCode.Forbidden);
        }


        var response =
            request.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(new
        {
            authenticated = true,
            tenantId,
            objectId,
            name = principal.GetDisplayName(),
            email = principal.GetEmail(),
            roles = principal.Claims
                .Where(c =>
                    c.Type.Equals(
                        "roles",
                        StringComparison.OrdinalIgnoreCase)
                    || c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .Distinct()
        });

        return response;
    }
}
