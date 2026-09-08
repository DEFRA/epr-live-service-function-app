using System.Data;
using Microsoft.Extensions.Configuration;
using EPR.LiveService.FunctionApp.Queries;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Queries;

[TestClass]
public class QueryResultLimitTests
{
    private static QueryResultLimit CreateLimit(string? value = null)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            value is null ? [] : new Dictionary<string, string?>
            {
                ["QueryResults:MaxRows"] = value
            }).Build();
        return new QueryResultLimit(configuration);
    }

    [TestMethod]
    public void MissingSetting_DefaultsTo1000()
    {
        CreateLimit().MaxRows.Should().Be(1000);
    }

    [TestMethod]
    public void ConfiguredLimit_IsUsedForDatabaseExecution()
    {
        var limit = CreateLimit("250");
        limit.MaxRows.Should().Be(250);
        limit.Apply("SELECT 1;").Should()
            .Be("SET ROWCOUNT 251;\nSELECT 1;\n;SET ROWCOUNT 0;");
    }

    [DataTestMethod]
    [DataRow("0")]
    [DataRow("-1")]
    [DataRow("2147483647")]
    public void UnsafeLimits_AreRejected(string value)
    {
        Action create = () => CreateLimit(value);
        create.Should().Throw<ArgumentOutOfRangeException>();
    }

    [DataTestMethod]
    [DataRow("abc")]
    [DataRow("1.5")]
    [DataRow("2147483648")]
    [DataRow("")]
    public void InvalidSettings_AreRejected(string value)
    {
        Action create = () => CreateLimit(value);
        create.Should().Throw<InvalidOperationException>();
    }

    [DataTestMethod]
    [DataRow(0, 0, 1000)]
    [DataRow(1000, 1000, 1000)]
    [DataRow(1001, 1001, 1000)]
    [DataRow(10000, 1001, 1000)]
    [DataRow(10, 4, 3)]
    [DataRow(3000, 2510, 2509)]
    public void Read_BoundsRowsAndPreservesValues(int count, int expected, int maxRows)
    {
        var table = new DataTable();
        table.Columns.Add("Email", typeof(string));
        for (var i = 0; i < count; i++)
            table.Rows.Add($"user{i}@example.com");
        using var reader = table.CreateDataReader();

        var rows = CreateLimit(maxRows.ToString()).Read(reader);

        rows.Should().HaveCount(expected);
        if (expected > 0)
            ((string)rows[0].Email).Should().Be("user0@example.com");
        if (count > expected)
        {
            reader.Read().Should().BeTrue();
            reader.GetString(0).Should().Be($"user{expected}@example.com");
        }
    }

    [TestMethod]
    public void Apply_LimitsDatabaseResultsAndResetsSessionSetting()
    {
        CreateLimit().Apply("SELECT 1;").Should()
            .Be("SET ROWCOUNT 1001;\nSELECT 1;\n;SET ROWCOUNT 0;");
    }

    [TestMethod]
    public async Task UserLookup_EscapesLikePatternsAndOnlyAllowsAsciiTable()
    {
        var registry = new QueryRegistry();
        registry.Get("user_lookup").Outputs.Should()
            .BeEquivalentTo(new[] { QueryOutputFormat.AsciiTable });
        var sql = await registry.LoadScriptAsync("user_lookup");
        sql.Should().Contain("'\\', '\\\\'")
            .And.Contain("'%', '\\%'")
            .And.Contain("'_', '\\_'")
            .And.Contain("'[', '\\['")
            .And.Contain("ESCAPE '\\'")
            .And.Contain("Email = @Email");
    }
}
