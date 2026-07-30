using EPR.LiveService.FunctionApp.Authorisation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;

namespace EPR.LiveService.FunctionApp.Middleware;

[ExcludeFromCodeCoverage]
public sealed class FunctionAuthorizationMiddleware(
    ILogger<FunctionAuthorizationMiddleware> logger) : IFunctionsWorkerMiddleware
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

        if (!attribute.IsAuthorized(principal))
        {
            logger.LogWarning(
                "Request rejected because its tenant, object identifier, or roles " +
                "do not satisfy the authorization requirements for function {FunctionName}.",
                context.FunctionDefinition.Name);

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
