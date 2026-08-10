using Microsoft.Extensions.Configuration;

namespace EPR.LiveService.FunctionApp.Configs;

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
