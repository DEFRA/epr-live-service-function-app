using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace EPR.LiveService.FunctionApp.Middleware
{
    [ExcludeFromCodeCoverage]
    public sealed class LocalClientPrincipalMiddleware : IFunctionsWorkerMiddleware
    {
        private const string ClientPrincipalHeader = "X-MS-CLIENT-PRINCIPAL";

        private static readonly string EncodedClientPrincipal = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(new
                {
                    auth_typ = "aad",
                    name_typ = "name",
                    role_typ = "roles",
                    claims = new[]
                    {
                        new { typ = "name", val = "Local Developer" },
                        new { typ = "preferred_username", val = "local.developer@example.com" },
                        new { typ = "http://schemas.microsoft.com/identity/claims/tenantid", val = "6f504113-6b64-43f2-ade9-242e05780007" },
                        new { typ = "http://schemas.microsoft.com/identity/claims/objectidentifier", val = "00000000-0000-0000-0000-000000000001" },
                        new { typ = "roles", val = "user" },
                        new { typ = "roles", val = "admin" }
                    }
                })));

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var request = await context.GetHttpRequestDataAsync();

            if (request is not null &&
                !request.Headers.Contains(ClientPrincipalHeader))
            {
                request.Headers.Add(ClientPrincipalHeader, EncodedClientPrincipal);
            }

            await next(context);
        }
    }
}
