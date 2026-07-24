using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace EPR.LiveService.FunctionApp.Middleware;

[ExcludeFromCodeCoverage]
public sealed class AuthClaimsLoggingMiddleware(
    ILogger<AuthClaimsLoggingMiddleware> logger,
    IClientPrincipalDecoder clientPrincipalDecoder) : IFunctionsWorkerMiddleware
{
    private const string BearerPrefix = "Bearer ";
    private const string ClientPrincipalHeader = "X-MS-CLIENT-PRINCIPAL";

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var request = await context.GetHttpRequestDataAsync();

        if (request is not null)
        {
            var hasClientPrincipal = request.Headers.TryGetValues(
                ClientPrincipalHeader,
                out var clientPrincipalHeaders);

            if (hasClientPrincipal && clientPrincipalHeaders is not null)
            {
                LogClientPrincipalClaims(clientPrincipalHeaders.FirstOrDefault());
            }
            else if (request.Headers.TryGetValues("Authorization", out var authorizationHeaders))
            {
                LogJwtClaims(authorizationHeaders.FirstOrDefault());
            }
        }

        await next(context);
    }

    private void LogClientPrincipalClaims(string? encodedClientPrincipal)
    {
        if (!clientPrincipalDecoder.TryDecode(encodedClientPrincipal, out var claims))
        {
            logger.LogWarning(
                "The request {ClientPrincipalHeader} header could not be decoded.",
                ClientPrincipalHeader);
            return;
        }

        foreach (var claim in claims)
        {
            logger.LogInformation(
                "Request authentication claim: {ClaimType} = {ClaimValue}",
                claim.Type,
                claim.Value);
        }
    }

    private void LogJwtClaims(string? authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            !authorizationHeader.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("Request contains no bearer token in Authorization header.");
            return;
        }

        var token = authorizationHeader[BearerPrefix.Length..].Trim();
        var tokenHandler = new JwtSecurityTokenHandler();

        if (!tokenHandler.CanReadToken(token))
        {
            logger.LogWarning("The request Authorization header does not contain a readable JWT.");
            return;
        }

        try
        {
            var jwt = tokenHandler.ReadJwtToken(token);

            foreach (var claim in jwt.Claims)
            {
                logger.LogInformation(
                    "Request authentication claim: {ClaimType} = {ClaimValue}",
                    claim.Type,
                    claim.Value);
            }
        }
        catch (ArgumentException exception)
        {
            logger.LogWarning(exception, "The request JWT could not be read.");
        }
    }
}
