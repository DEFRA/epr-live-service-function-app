using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace EPR.LiveService.FunctionApp.Configs;

[ExcludeFromCodeCoverage]
public class ApiEndpoint
{
    public const string SectionName = "ApiEndpoint";

    private ApiEndpoint(
        string regulatorOrganisationApproval)
    {
        RegulatorOrganisationApproval = regulatorOrganisationApproval;
    }

    public string RegulatorOrganisationApproval { get; }


    public static ApiEndpoint FromConfiguration(IConfigurationSection configuration)
    {
        var regulatorOrganisationApproval = configuration.GetRequiredValue(
            nameof(RegulatorOrganisationApproval));

        return new ApiEndpoint(regulatorOrganisationApproval);
    }
}
