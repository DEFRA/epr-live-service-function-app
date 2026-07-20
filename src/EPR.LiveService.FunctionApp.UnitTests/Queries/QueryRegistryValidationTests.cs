using System.Text.RegularExpressions;
using EPR.LiveService.FunctionApp.Queries;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Queries;

/// <summary>
/// Validates the shape of every registered query — the Definitions/*.json and
/// Scripts/*.sql pairs that make up the registry — rather than testing execution
/// logic. These are the cheapest, highest-value tests for this codebase: the
/// registry pattern's whole premise is "add a query = 2 files, no code change",
/// so config mistakes (not logic bugs) are the most likely failure mode.
/// </summary>
[TestClass]
public partial class QueryRegistryValidationTests
{
    /// <summary>
    /// The set of SQL targets currently wired up in Program.cs. Kept here as an
    /// explicit list rather than derived from configuration, since the registry
    /// itself has no dependency on SqlTargets. Update this list whenever a new
    /// target is registered in Program.cs — if it drifts, this test starts
    /// producing false positives/negatives rather than catching real mistakes,
    /// so it's the one piece of this file worth keeping in sync deliberately.
    /// </summary>
    private static readonly HashSet<string> KnownTargets = new(StringComparer.OrdinalIgnoreCase)
    {
        "accounts",
        "synapse"
    };

    private static readonly HashSet<string> AllowedParameterTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "text",
        "number",
        "date",
    };

    private QueryRegistry _registry = null!;

    [TestInitialize]
    public void Setup()
    {
        // Constructing QueryRegistry already runs its own startup validation
        // (every definition has a matching script) — if that throws, every
        // test in this class fails immediately with a clear message, which
        // is exactly the fail-fast behaviour we want in CI too.
        _registry = new QueryRegistry();
    }

    [TestMethod]
    public void Registry_ShouldHaveAtLeastOneQueryRegistered()
    {
        _registry.All().Should().NotBeEmpty("the registry should never be deployed empty");
    }

    [TestMethod]
    public async Task EveryDefinition_ShouldHaveAMatchingSqlScript()
    {
        foreach (var definition in _registry.All())
        {
            var act = async () => await _registry.LoadScriptAsync(definition.Id);
    
            await act.Should().NotThrowAsync<FileNotFoundException>(
                $"query '{definition.Id}' has a definition but no matching .sql script file");
        }
    }

    [TestMethod]
    public void EveryDefinition_ShouldHaveANonEmptyId()
    {
        foreach (var definition in _registry.All())
        {
            definition.Id.Should().NotBeNullOrWhiteSpace();
        }
    }

    [TestMethod]
    public void EveryDefinition_ShouldTargetAKnownSqlTarget()
    {
        foreach (var definition in _registry.All())
        {
            KnownTargets.Should().Contain(
                definition.Target,
                $"query '{definition.Id}' declares target '{definition.Target}', " +
                "which must match a target configured in Program.cs / SqlTargets");
        }
    }

    [TestMethod]
    public void EveryParameter_ShouldHaveAnAllowedType()
    {
        foreach (var definition in _registry.All())
        {
            foreach (var parameter in definition.Parameters)
            {
                AllowedParameterTypes.Should().Contain(
                    parameter.Type,
                    $"query '{definition.Id}' parameter '{parameter.Name}' has type " +
                    $"'{parameter.Type}', which is not one of the supported types");
            }
        }
    }

    [TestMethod]
    public void EveryParameter_ShouldHaveANonEmptyLabel()
    {
        foreach (var definition in _registry.All())
        {
            foreach (var parameter in definition.Parameters)
            {
                parameter.Label.Should().NotBeNullOrWhiteSpace(
                    $"query '{definition.Id}' parameter '{parameter.Name}' needs a label for the generated form");
            }
        }
    }

    [TestMethod]
    public void EveryDefinition_ShouldDeclareAtLeastOneOutput()
    {
        foreach (var definition in _registry.All())
        {
            definition.Outputs.Should().NotBeEmpty(
                $"query '{definition.Id}' must declare at least one output, or the form has " +
                "nothing to submit as the 'output' value");
        }
    }

    // No "every output should be a known format" test here: Outputs is a
    // List<QueryOutputFormat>, so an invalid value in a .json definition
    // (e.g. "pdf") fails QueryOutputFormatJsonConverter deserialization when
    // QueryRegistry is constructed in Setup() above, before any test body runs.

    [TestMethod]
    public async Task EveryScript_ShouldOnlyReferencePlaceholdersDeclaredInItsDefinition()
    {
        foreach (var definition in _registry.All())
        {
            var sql = await _registry.LoadScriptAsync(definition.Id);
            var placeholders = ExtractSqlParameterNames(sql);
            var declaredNames = definition.Parameters
                .Select(p => p.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var undeclared = placeholders
                .Where(p => !declaredNames.Contains(p))
                .ToList();

            undeclared.Should().BeEmpty(
                $"query '{definition.Id}' references SQL parameter(s) not declared in its " +
                $".json definition: {string.Join(", ", undeclared)}. This would fail at " +
                "execution time with an 'unknown parameter' error from Dapper/SqlClient.");
        }
    }

    [TestMethod]
    public async Task EveryDeclaredParameter_ShouldBeUsedSomewhereInTheScript()
    {
        foreach (var definition in _registry.All())
        {
            var sql = await _registry.LoadScriptAsync(definition.Id);
            var placeholders = ExtractSqlParameterNames(sql);

            var unused = definition.Parameters
                .Select(p => p.Name)
                .Where(name => !placeholders.Contains(name, StringComparer.OrdinalIgnoreCase))
                .ToList();

            unused.Should().BeEmpty(
                $"query '{definition.Id}' declares parameter(s) never referenced in its .sql " +
                $"script: {string.Join(", ", unused)}. Likely a leftover from editing the query.");
        }
    }

    [TestMethod]
    public void Get_WithUnknownId_ThrowsKeyNotFoundException()
    {
        var act = () => _registry.Get("does-not-exist");
    
        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*does-not-exist*");
    }

    [TestMethod]
    public async Task LoadScriptAsync_WithUnknownQueryId_ThrowsFileNotFoundException()
    {
        var act = async () => await _registry.LoadScriptAsync("does-not-exist");
    
        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("*not found for query*");
    }

    /// <summary>
    /// Extracts SQL parameter placeholders (e.g. "ReferenceNumber" from "@ReferenceNumber"),
    /// excluding SQL Server system variables like @@ROWCOUNT.
    /// </summary>
    [GeneratedRegex(@"(?<!@)@(\w+)")]
    private static partial Regex SqlParameterRegex();

    private static HashSet<string> ExtractSqlParameterNames(string sql)
    {
        var matches = SqlParameterRegex().Matches(sql);

        return matches
            .Select(m => m.Groups[1].Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}