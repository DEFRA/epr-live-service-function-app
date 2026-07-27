using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EPR.LiveService.FunctionApp.Middleware;

public sealed class ClientPrincipalDecoder : IClientPrincipalDecoder
{
    public bool TryDecode(
        string? encodedClientPrincipal,
        out IReadOnlyList<ClientPrincipalClaim> claims)
    {
        claims = [];

        if (string.IsNullOrWhiteSpace(encodedClientPrincipal))
        {
            return false;
        }

        try
        {
            var json = Encoding.UTF8.GetString(
                Convert.FromBase64String(encodedClientPrincipal));
            var principal = JsonSerializer.Deserialize<ClientPrincipal>(json);

            if (principal is null)
            {
                return false;
            }

            claims = principal.Claims;
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private sealed class ClientPrincipal
    {
        [JsonPropertyName("claims")]
        public IReadOnlyList<ClientPrincipalClaim> Claims { get; init; } = [];
    }
}

public sealed record ClientPrincipalClaim(
    [property: JsonPropertyName("typ")] string Type,
    [property: JsonPropertyName("val")] string Value);
