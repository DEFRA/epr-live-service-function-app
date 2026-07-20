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
    public void QueryFormPage_ShouldRenderBackToQueriesLink()
    {
        var definition = new QueryDefinition
        {
            Id = "organisation_details",
            DisplayName = "Organisation details",
            Description = "Find an organisation",
            Parameters = []
        };

        var html = QueryFormPage.Build(definition);

        html.Should().Contain("href=\"/api/queries\"");
    }

    [TestMethod]
    public void QueryFormPage_WithMultipleOutputs_ShouldRenderRadioButtonsInDeclaredOrder()
    {
        var definition = new QueryDefinition
        {
            Id = "organisation_details",
            DisplayName = "Organisation details",
            Description = "Find an organisation",
            Parameters = [],
            Outputs = [QueryOutputFormat.Csv, QueryOutputFormat.Html]
        };

        var html = QueryFormPage.Build(definition);

        html.Should().Contain("<legend>Output format</legend>");
        html.Should().Contain("value=\"csv\" checked");
        html.Should().Contain("value=\"html\"");
        html.Should().NotContain("type=\"hidden\" name=\"output\"");
    }

    [TestMethod]
    public void QueryFormPage_WithSingleOutput_ShouldRenderHiddenFieldInsteadOfRadios()
    {
        var definition = new QueryDefinition
        {
            Id = "registration_submission_events",
            DisplayName = "Registration submission events",
            Description = "A big report",
            Parameters = [],
            Outputs = [QueryOutputFormat.Csv]
        };

        var html = QueryFormPage.Build(definition);

        html.Should().Contain("<input type=\"hidden\" name=\"output\" value=\"csv\" />");
        html.Should().NotContain("<legend>Output format</legend>");
        html.Should().NotContain("type=\"radio\" name=\"output\"");
    }

    [TestMethod]
    public void EveryQueryOutputFormat_ShouldHaveDisplayMetadata()
    {
        // With the attribute-based metadata, there's no compiler warning if a
        // new QueryOutputFormat value is missing its [QueryOutputFormatMetadata]
        // attribute — this test is the actual safety net: it fails in CI rather
        // than the first time someone opens that format's form.
        foreach (var format in Enum.GetValues<QueryOutputFormat>())
        {
            format.DisplayName().Should().NotBeNullOrWhiteSpace();
            format.Hint().Should().NotBeNullOrWhiteSpace();
            format.Key().Should().NotBeNullOrWhiteSpace();
        }
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
