using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace EPR.LiveService.FunctionApp.Middleware;

[ExcludeFromCodeCoverage]
public sealed class AuthClaimsLoggingMiddleware(
    ILogger<AuthClaimsLoggingMiddleware> logger) : IFunctionsWorkerMiddleware
{
    private const string BearerPrefix = "Bearer ";

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var request = await context.GetHttpRequestDataAsync();

        if (request is not null &&
            request.Headers.TryGetValues("Authorization", out var authorizationHeaders))
        {
            LogClaims(authorizationHeaders.FirstOrDefault());
        }

        await next(context);
    }

    private void LogClaims(string? authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            !authorizationHeader.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
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
