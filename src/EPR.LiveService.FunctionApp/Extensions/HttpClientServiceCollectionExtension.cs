using Azure.Core;
using Azure.Identity;
using EPR.LiveService.FunctionApp.Configs;
using EPR.LiveService.FunctionApp.Handlers;
using EPR.LiveService.FunctionApp.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.API.Extensions;

[ExcludeFromCodeCoverage]
public static class HttpClientServiceCollectionExtension
{
    public static IServiceCollection AddServicesAndHttpClients(this IServiceCollection services)
    {
        services.AddSingleton<TokenCredential>(new DefaultAzureCredential());
        services.AddTransient<OrganisationServiceAuthorisationHandler>();

        services.AddHttpClient<IOrganisationService, OrganisationService>((sp, client) =>
        {
            var config = sp.GetRequiredService<ApiConfig>();

            client.BaseAddress = new Uri(config.OrganisationServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.Timeout);
        })
        .AddHttpMessageHandler<OrganisationServiceAuthorisationHandler>();

        return services;
    }
}
