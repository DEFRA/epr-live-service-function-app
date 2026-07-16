using System.Dynamic;
using EPR.LiveService.FunctionApp.Formatting;
using EPR.LiveService.FunctionApp.Queries;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Formatting;

[TestClass]
public class TemplateRenderingTests
{
    [TestMethod]
    public void QueryFormPage_ShouldRenderSharedStylesPartial()
    {
        var definition = new QueryDefinition
        {
            Id = "organisation_details",
            DisplayName = "Organisation details",
            Description = "Find an organisation",
            Parameters =
            [
                new QueryParameterDefinition
                {
                    Name = "OrganisationId",
                    Label = "Organisation ID",
                    Type = "number"
                }
            ]
        };

        var html = QueryFormPage.Build(definition);

        html.Should().Contain("<style>");
        html.Should().Contain("font-family: system-ui, sans-serif;");
        html.Should().Contain("Organisation details");
        html.Should().Contain("""const queryId = "organisation_details";""");
        html.Should().Contain("type=\"number\"");
    }

    [TestMethod]
    public void HtmlTableFormatter_ShouldRenderEscapedTableFragment()
    {
        dynamic row = new ExpandoObject();
        row.Name = "<script>alert(1)</script>";

        var html = HtmlTableFormatter.ToHtmlTable(new[] { row });

        html.Should().Contain("id=\"query-results-list\"");
        html.Should().Contain("&lt;script&gt;alert(1)&lt;/script&gt;");
        html.Should().NotContain("<script>alert(1)</script>");
    }

    [TestMethod]
    public void AsciiTableFormatter_ShouldRenderEscapedPreFragment()
    {
        var html = AsciiTableFormatter.WrapAsFragment("<unsafe>");

        html.Should().Be("<pre>&lt;unsafe&gt;</pre>");
    }
}
