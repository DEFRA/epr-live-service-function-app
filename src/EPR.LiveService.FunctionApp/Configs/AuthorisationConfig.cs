using System.Diagnostics.CodeAnalysis;
using EPR.LiveService.FunctionApp.Extensions;
using Microsoft.Extensions.Configuration;

namespace EPR.LiveService.FunctionApp.Configs;

[ExcludeFromCodeCoverage]
public class AuthorisationConfig
{
    public const string SectionName = "AzureAd";

    private AuthorisationConfig(string tenantId)
    {
        TenantId = tenantId;
    }

    public string TenantId { get; }

    public static AuthorisationConfig FromConfiguration(IConfigurationSection configuration)
    {
        var tenantId = configuration.GetRequiredValue(nameof(TenantId));

        if (!Guid.TryParse(tenantId, out _))
        {
            throw new InvalidOperationException(
                $"Configuration value '{configuration.Path}:{nameof(TenantId)}' must be a valid GUID.");
        }

        return new AuthorisationConfig(tenantId);
    }
}