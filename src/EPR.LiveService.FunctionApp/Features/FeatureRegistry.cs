using System.Reflection;
using System.Text.Json;

namespace EPR.LiveService.FunctionApp.Features;

public class FeatureRegistry : IFeatureRegistry
{
    private const string DefinitionsResourceNamespace = "Features.Definitions";
    private static readonly JsonSerializerOptions DeserializationOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IReadOnlyList<FeatureDefinition> _definitions;

    public FeatureRegistry()
    {
        var assembly = typeof(FeatureRegistry).Assembly;
        _definitions = assembly.GetManifestResourceNames()
            .Where(name => name.Contains(DefinitionsResourceNamespace, StringComparison.Ordinal)
                           && name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .Select(resourceName => LoadDefinition(assembly, resourceName))
            .ToArray();
    }

    public IEnumerable<FeatureDefinition> All() => _definitions;

    private static FeatureDefinition LoadDefinition(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        return JsonSerializer.Deserialize<FeatureDefinition>(stream, DeserializationOptions)
            ?? throw new InvalidOperationException(
            $"Failed to deserialize feature definition from '{resourceName}'");
    }
}
