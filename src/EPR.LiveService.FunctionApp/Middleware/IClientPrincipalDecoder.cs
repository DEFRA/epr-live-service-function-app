namespace EPR.LiveService.FunctionApp.Middleware;

public interface IClientPrincipalDecoder
{
    bool TryDecode(
        string? encodedClientPrincipal,
        out IReadOnlyList<ClientPrincipalClaim> claims);
}
