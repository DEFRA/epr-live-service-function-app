using System.Data;
using EPR.LiveService.FunctionApp.Queries;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Queries;

[TestClass]
public class QueryResultLimitTests
{
    [DataTestMethod]
    [DataRow(0, 0)]
    [DataRow(100, 100)]
    [DataRow(101, 101)]
    [DataRow(1000, 101)]
    public void Read_BoundsRowsAndPreservesValues(int count, int expected)
    {
        var table = new DataTable();
        table.Columns.Add("Email", typeof(string));
        for (var i = 0; i < count; i++)
            table.Rows.Add($"user{i}@example.com");
        using var reader = table.CreateDataReader();

        var rows = QueryResultLimit.Read(reader);

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
        QueryResultLimit.Apply("SELECT 1;").Should()
            .Be("SET ROWCOUNT 101;\nSELECT 1;\n;SET ROWCOUNT 0;");
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
