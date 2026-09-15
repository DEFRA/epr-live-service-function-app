using System.Dynamic;
using EPR.LiveService.FunctionApp.Formatting;
using EPR.LiveService.FunctionApp.UnitTests.TestSupport.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Formatting;

[TestClass]
public class HtmlTableFormatterTests
{
    [TestMethod]
    public async Task WriteAsync_ShouldWriteRenderedHtmlTable()
    {
        dynamic row = new ExpandoObject();
        row.Name = "Joe";
        var response = TestHttpResponseData.Create(new TestFunctionContext());

        await new HtmlTableFormatter().WriteAsync(response, "user_lookup", new[] { row });

        response.ReadBodyAsString().Should().Contain("Joe");
    }
}
