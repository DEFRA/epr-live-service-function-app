using System.Diagnostics.CodeAnalysis;
using EPR.LiveService.FunctionApp.Extensions;
using Microsoft.Extensions.Configuration;

namespace EPR.LiveService.FunctionApp.Configs;

[ExcludeFromCodeCoverage]
public class ApiConfig
{
    public const string SectionName = "ApiConfig";

    private ApiConfig(
        string organisationServiceBaseUrl,
        string organisationServiceClientId,
        int timeout)
    {
        OrganisationServiceBaseUrl = organisationServiceBaseUrl;
        OrganisationServiceClientId = organisationServiceClientId;
        Timeout = timeout;
    }

    public string OrganisationServiceBaseUrl { get; }

    public string OrganisationServiceClientId { get; }


    public int Timeout { get; }

    public static ApiConfig FromConfiguration(IConfigurationSection configuration)
    {
        var organisationServiceBaseUrl = configuration.GetRequiredValue(
            nameof(OrganisationServiceBaseUrl));
        var organisationServiceClientId = configuration.GetRequiredValue(
            nameof(OrganisationServiceClientId));
        var timeout = configuration.GetValue<int?>(nameof(Timeout));

        if (timeout is null or <= 0)
        {
            throw new InvalidOperationException(
                $"Configuration value '{configuration.Path}:{nameof(Timeout)}' must be populated with a positive integer.");
        }

        return new ApiConfig(
            organisationServiceBaseUrl,
            organisationServiceClientId,
            timeout.Value);
    }
}
