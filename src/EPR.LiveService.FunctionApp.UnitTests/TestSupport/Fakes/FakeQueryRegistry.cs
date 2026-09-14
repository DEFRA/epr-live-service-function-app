using EPR.LiveService.FunctionApp.Queries;

namespace EPR.LiveService.FunctionApp.UnitTests.TestSupport.Fakes;

internal sealed class FakeQueryRegistry : IQueryRegistry
{
    private readonly Dictionary<string, QueryDefinition> _definitions = new(StringComparer.OrdinalIgnoreCase);

    public void Add(QueryDefinition definition) => _definitions[definition.Id] = definition;

    public QueryDefinition Get(string id) =>
        _definitions.TryGetValue(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"No query registered with id '{id}'.");

    public IEnumerable<QueryDefinition> All() => _definitions.Values;

    public Task<string> LoadScriptAsync(string queryId) => Task.FromResult("SELECT 1");
}
