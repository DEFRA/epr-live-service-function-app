using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace EPR.LiveService.FunctionApp.Configs;

[ExcludeFromCodeCoverage]
public class ApiEndpoints
{
    public const string SectionName = "ApiEndpoints";

    private ApiEndpoints(
        string regulatorOrganisationApproval)
    {
        RegulatorOrganisationApproval = regulatorOrganisationApproval;
    }

    public string RegulatorOrganisationApproval { get; }


    public static ApiEndpoints FromConfiguration(IConfigurationSection configuration)
    {
        var regulatorOrganisationApproval = configuration.GetRequiredValue(
            nameof(RegulatorOrganisationApproval));

        return new ApiEndpoints(regulatorOrganisationApproval);
    }
}
