using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace EPR.LiveService.FunctionApp.Extensions;

[ExcludeFromCodeCoverage]
internal static class ConfigurationSectionExtensions
{
    public static string GetRequiredValue(
        this IConfigurationSection configuration,
        string key)
    {
        var value = configuration[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Configuration value '{configuration.Path}:{key}' must be populated.");
        }

        return value;
    }
}
