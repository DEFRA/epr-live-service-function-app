using EPR.LiveService.FunctionApp.Configs;
using EPR.LiveService.FunctionApp.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Security.Claims;

namespace EPR.LiveService.FunctionApp.Middleware;

[ExcludeFromCodeCoverage(Justification = "Isolated-worker middleware — needs a full FunctionContext/FunctionExecutionDelegate pipeline to exercise meaningfully; not unit-testable without a Functions host.")]
public sealed class FunctionAuthorizationMiddleware(
    ILogger<FunctionAuthorizationMiddleware> logger,
    AuthorisationConfig authorisationConfig) : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var attribute = GetAuthorizationAttribute(context);

        if (attribute is null)
        {
            await next(context);
            return;
        }

        var request = await context.GetHttpRequestDataAsync();

        if (request is null)
        {
            await next(context);
            return;
        }

        var principal = EasyAuthPrincipal.Parse(request, logger);

        if (principal?.Identity?.IsAuthenticated != true)
        {
            logger.LogWarning(
                "Unauthenticated request rejected for function {FunctionName}.",
                context.FunctionDefinition.Name);

            context.GetInvocationResult().Value =
                request.CreateResponse(HttpStatusCode.Unauthorized);

            return;
        }

        if (!attribute.IsAuthorized(principal, authorisationConfig.TenantId))
        {
            logger.LogWarning(
                "Authorization failed for {FunctionName}. TokenTenant={TokenTenant} ExpectedTenant={ExpectedTenant} HasObjectId={HasObjectId} Roles={Roles}",
                context.FunctionDefinition.Name,
                principal.GetTenantId(),
                authorisationConfig.TenantId,
                !string.IsNullOrWhiteSpace(principal.GetObjectId()),
                string.Join(",", principal.Claims
                    .Where(c => c.Type is "roles" or "role" or ClaimTypes.Role)
                    .Select(c => c.Value)));

            context.GetInvocationResult().Value =
                request.CreateResponse(HttpStatusCode.Forbidden);

            return;
        }

        await next(context);
    }

    private static AuthorizeFunctionAttribute? GetAuthorizationAttribute(
        FunctionContext context)
    {
        var entryPoint = context.FunctionDefinition.EntryPoint;
        var methodSeparator = entryPoint.LastIndexOf('.');

        if (methodSeparator < 1)
        {
            return null;
        }

        var typeName = entryPoint[..methodSeparator];
        var methodName = entryPoint[(methodSeparator + 1)..];
        var method = typeof(FunctionAuthorizationMiddleware).Assembly
            .GetType(typeName)?
            .GetMethod(
                methodName,
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.Static);

        return method?.GetCustomAttribute<AuthorizeFunctionAttribute>();
    }
}
