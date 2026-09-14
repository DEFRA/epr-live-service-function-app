using System.Dynamic;
using EPR.LiveService.FunctionApp.Formatting;
using EPR.LiveService.FunctionApp.UnitTests.TestSupport.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Formatting;

[TestClass]
public class AsciiTableFormatterTests
{
    [TestMethod]
    public void ToAsciiTable_WithNoRows_ShouldReturnPlaceholder()
    {
        var result = AsciiTableFormatter.ToAsciiTable(Array.Empty<dynamic>());

        result.Should().Be("(no rows)");
    }

    [TestMethod]
    public void ToAsciiTable_ShouldRenderColumnHeadersAndValues()
    {
        dynamic row = new ExpandoObject();
        row.Name = "Jo";
        row.Age = "40";

        var table = AsciiTableFormatter.ToAsciiTable(new[] { row });

        table.Should().Contain("Name");
        table.Should().Contain("Age");
        table.Should().Contain("Jo");
        table.Should().Contain("40");
    }

    [TestMethod]
    public void ToAsciiTable_ShouldFrameTheTableWithTopMiddleAndBottomSeparators()
    {
        dynamic first = new ExpandoObject();
        first.Name = "Alice";
        dynamic second = new ExpandoObject();
        second.Name = "Bob";

        var table = AsciiTableFormatter.ToAsciiTable(new[] { first, second });
        var lines = table.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Top, header/body divider, bottom — always 3, regardless of row count.
        lines.Count(line => line.StartsWith('+')).Should().Be(3);
        lines.Should().Contain(line => line.Contains("Alice"));
        lines.Should().Contain(line => line.Contains("Bob"));
    }

    [TestMethod]
    public void ToAsciiTable_ShouldRenderNullValuesAsNULL()
    {
        dynamic row = new ExpandoObject();
        row.Value = null;

        var table = AsciiTableFormatter.ToAsciiTable(new[] { row });

        table.Should().Contain("NULL");
    }

    [TestMethod]
    public void ToAsciiTable_ShouldWidenColumnToFitLongestValueRatherThanClipIt()
    {
        dynamic row = new ExpandoObject();
        row.Id = "A very long value here";

        var table = AsciiTableFormatter.ToAsciiTable(new[] { row });
        var lines = table.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Every line ends up the same width — proves the column widened to
        // fit the value rather than the value being clipped to the header.
        lines.Select(line => line.Length).Distinct().Should().HaveCount(1);
        table.Should().Contain("A very long value here");
    }

    [TestMethod]
    public async Task WriteAsync_ShouldWriteAsciiTableWrappedAsHtmlFragment()
    {
        dynamic row = new ExpandoObject();
        row.Name = "Joe";
        var response = TestHttpResponseData.Create(new TestFunctionContext());
    
        await new AsciiTableFormatter().WriteAsync(response, "user_lookup", new[] { row });
    
        var html = response.ReadBodyAsString();
        html.Should().Contain("<pre>");
        html.Should().Contain("Joe");
    }
}
