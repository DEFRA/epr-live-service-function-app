using System.Text.Json;
using EPR.LiveService.FunctionApp.Formatting;
using EPR.LiveService.FunctionApp.Queries;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Formatting;

[TestClass]
public class QueryOutputFormatJsonConverterTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new QueryOutputFormatJsonConverter() }
    };

    [TestMethod]
    public void Write_ShouldSerializeUsingTheFormatsKey()
    {
        var json = JsonSerializer.Serialize(QueryOutputFormat.AsciiTable, Options);

        json.Should().Be("\"ascii_table\"");
    }

    [TestMethod]
    public void Read_WithUnrecognisedKey_ShouldThrowJsonException()
    {
        var act = () => JsonSerializer.Deserialize<QueryOutputFormat>("\"pdf\"", Options);

        act.Should().Throw<JsonException>().WithMessage("*pdf*");
    }
}
