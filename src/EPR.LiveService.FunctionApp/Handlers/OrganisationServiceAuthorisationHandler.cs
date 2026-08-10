using Azure.Core;
using Azure.Identity;
using EPR.LiveService.FunctionApp.Configs;
using Microsoft.Identity.Web;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace EPR.LiveService.FunctionApp.Handlers;

[ExcludeFromCodeCoverage]
public class OrganisationServiceAuthorisationHandler : DelegatingHandler
{
    private readonly TokenRequestContext _tokenRequestContext;

    private readonly DefaultAzureCredential _credentials;

    public OrganisationServiceAuthorisationHandler(ApiConfig config)
    {
        _tokenRequestContext = new TokenRequestContext([config.OrganisationServiceClientId]);
        _credentials = new DefaultAzureCredential();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization is null)
        {
            var tokenResult = await _credentials.GetTokenAsync(
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
