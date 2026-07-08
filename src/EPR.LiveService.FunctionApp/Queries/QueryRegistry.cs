using System.Reflection;
using System.Text.Json;

namespace EPR.LiveService.FunctionApp.Queries;

/// <summary>
/// Loads QueryDefinitions from embedded Queries/Definitions/*.json resources and
/// matches each one to a Queries/Scripts/{id}.sql resource. Adding a new query is
/// purely a matter of dropping a new .json + .sql pair — no code change here.
/// </summary>
public class QueryRegistry : IQueryRegistry
{

    private const string DefinitionsResourceNamespace = "Queries.Definitions";
    private const string ScriptsResourceNamespace = "Queries.Scripts";

    private static readonly JsonSerializerOptions DeserializationOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Assembly _assembly = typeof(QueryRegistry).Assembly;
    private readonly Dictionary<string, QueryDefinition> _definitions;

    public QueryRegistry()
    {
        _definitions = _assembly.GetManifestResourceNames()
            .Where(name => name.Contains(DefinitionsResourceNamespace, StringComparison.Ordinal)
                           && name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .Select(LoadDefinition)
            .ToDictionary(d => d.Id, StringComparer.OrdinalIgnoreCase);

        ValidateScriptsExist();
    }

    public QueryDefinition Get(string id) =>
        _definitions.TryGetValue(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"No query registered with id '{id}'");

    public IEnumerable<QueryDefinition> All() => _definitions.Values;

    public async Task<string> LoadScriptAsync(string queryId)
    {
        var resourceName = BuildScriptResourceName(queryId);

        await using var stream = _assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException(
                $"Script resource '{resourceName}' not found for query '{queryId}'");

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

   private QueryDefinition LoadDefinition(string resourceName)
    {
        using var stream = _assembly.GetManifestResourceStream(resourceName)!;
        var definition = JsonSerializer.Deserialize<QueryDefinition>(stream, DeserializationOptions);

        return definition ?? throw new InvalidOperationException(
            $"Failed to deserialize query definition from '{resourceName}'");
    }

    private string BuildScriptResourceName(string queryId) =>
        $"{_assembly.GetName().Name}.{ScriptsResourceNamespace}.{queryId}.sql";

    /// <summary>
    /// Fails fast at startup, rather than on first request, if a definition is
    /// missing its matching .sql file.
    /// </summary>
    private void ValidateScriptsExist()
    {
        var resourceNames = new HashSet<string>(
            _assembly.GetManifestResourceNames(), StringComparer.Ordinal);

        var missing = _definitions.Values
            .Where(d => !resourceNames.Contains(BuildScriptResourceName(d.Id)))
            .Select(d => d.Id)
            .ToList();

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Missing .sql script(s) for query definition(s): {string.Join(", ", missing)}");
        }
    }
}