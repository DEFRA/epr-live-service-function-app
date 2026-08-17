using Azure.Core;
using EPR.LiveService.FunctionApp.Configs;
using Microsoft.Identity.Web;
using System.Net.Http.Headers;

namespace EPR.LiveService.FunctionApp.Handlers;

public class OrganisationServiceAuthorisationHandler : DelegatingHandler
{
    private readonly TokenRequestContext _tokenRequestContext;

    private readonly TokenCredential _credential;

    public OrganisationServiceAuthorisationHandler(ApiConfig config, TokenCredential credential)
    {
        _tokenRequestContext = new TokenRequestContext([$"api://{config.OrganisationServiceClientId}/.default"]);
        _credential = credential;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization is null)
        {
            var tokenResult = await _credential.GetTokenAsync(
                _tokenRequestContext,
                cancellationToken);

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    Constants.Bearer,
                    tokenResult.Token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
