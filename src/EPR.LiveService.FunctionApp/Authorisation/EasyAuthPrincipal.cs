using Microsoft.Azure.Functions.Worker.Http;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace EPR.LiveService.FunctionApp.Authorisation;

[ExcludeFromCodeCoverage]
public static class EasyAuthPrincipal
{
    public static ClaimsPrincipal? Parse(
        HttpRequestData request,
        ILogger? logger = null)
    {
        if (!request.Headers.TryGetValues(
                "X-MS-CLIENT-PRINCIPAL",
                out var values))
        {
            return null;
        }

        var encodedPrincipal = values.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(encodedPrincipal))
        {
            logger?.LogWarning(
                "The X-MS-CLIENT-PRINCIPAL header was present but empty.");
            return null;
        }

        try
        {
            var decodedBytes =
                Convert.FromBase64String(encodedPrincipal);

            var json = Encoding.UTF8.GetString(decodedBytes);

            var clientPrincipal =
                JsonSerializer.Deserialize<ClientPrincipal>(json);

            if (clientPrincipal?.Claims is null)
            {
                logger?.LogWarning(
                    "The X-MS-CLIENT-PRINCIPAL header did not contain any claims.");
                return null;
            }

            var claims = clientPrincipal.Claims
                .Where(c =>
                    !string.IsNullOrWhiteSpace(c.Type) &&
                    c.Value is not null)
                .Select(c => new Claim(c.Type!, c.Value!));

            var identity = new ClaimsIdentity(
                claims,
                clientPrincipal.AuthenticationType,
                clientPrincipal.NameClaimType,
                clientPrincipal.RoleClaimType);

            return new ClaimsPrincipal(identity);
        }
        catch (FormatException exception)
        {
            logger?.LogWarning(
                exception,
                "The X-MS-CLIENT-PRINCIPAL header was not valid Base64.");
            return null;
        }
        catch (JsonException exception)
        {
            logger?.LogWarning(
                exception,
                "The X-MS-CLIENT-PRINCIPAL header did not contain valid JSON.");
            return null;
        }
    }

    private sealed class ClientPrincipal
    {
        [JsonPropertyName("auth_typ")]
        public string? AuthenticationType { get; init; }

        [JsonPropertyName("name_typ")]
        public string? NameClaimType { get; init; }

        [JsonPropertyName("role_typ")]
        public string? RoleClaimType { get; init; }

        [JsonPropertyName("claims")]
        public ClientPrincipalClaim[]? Claims { get; init; }
    }

    private sealed class ClientPrincipalClaim
    {
        [JsonPropertyName("typ")]
        public string? Type { get; init; }

        [JsonPropertyName("val")]
        public string? Value { get; init; }
    }
}
