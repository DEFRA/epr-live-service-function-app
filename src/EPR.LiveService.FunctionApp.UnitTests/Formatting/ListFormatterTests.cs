using System.Dynamic;
using EPR.LiveService.FunctionApp.Formatting;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Formatting;

[TestClass]
public class ListFormatterTests
{
    [TestMethod]
    public void ToHtmlList_ShouldRenderHumanReadableLabelsValuesAndLinks()
    {
        dynamic row = new ExpandoObject();
        row.TemplateLink = "https://notifications.gov/template/123";
        row.InvitedUserEmail = "joe.bloggs@company.com";
        row.InviterFirstName = "Jill";
        row.OrganisationName = "Kellbloggs";
        row.OrgRef = "101 234";
        row.MissingValue = null;

        var html = ListFormatter.ToHtmlList(new[] { row });

        html.Should().Contain("Template Link:");
        html.Should().Contain("Inviter First Name:");
        html.Should().Contain("Organisation Name:");
        html.Should().Contain("Invitee Email:");
        html.Should().Contain("Organisation ID:");
        html.Should().Contain("joe.bloggs@company.com");
        html.Should().Contain("Jill");
        html.Should().Contain("101 234");
        html.Should().Contain("NULL");
        html.Should().Contain("<a href=\"https://notifications.gov/template/123\">https://notifications.gov/template/123</a>");
        html.Should().Contain("class=\"copy-field-button\"");
        html.Should().Contain("data-copy-value=\"joe.bloggs@company.com\"");
        html.Should().Contain("aria-label=\"Copy Invitee Email\"");
        html.Should().Contain("<svg aria-hidden=\"true\"");
    }

    [TestMethod]
    public void ToHtmlList_ShouldOnlyOfferCopyForNonNullStringValues()
    {
        dynamic row = new ExpandoObject();
        row.Text = "copy me";
        row.Number = 42;
        row.Missing = null;

        var html = ListFormatter.ToHtmlList(new[] { row });

        html.Should().Contain("data-copy-value=\"copy me\"");
        html.Should().NotContain("data-copy-value=\"42\"");
        html.Should().NotContain("data-copy-value=\"NULL\"");
        html.Split("class=\"copy-field-button\"").Should().HaveCount(2);
    }

    [TestMethod]
    public void ToHtmlList_ShouldRenderGenericResultActions()
    {
        dynamic row = new ExpandoObject();
        row.Name = "Joe";

        var html = ListFormatter.ToHtmlList(
            new[] { row },
            [new QueryResultAction("Re-send invitation", "/api/resend?FirstName=Joe%20Bloggs")]);

        html.Should().Contain("class=\"button-link\"");
        html.Should().Contain("Re-send invitation");
        html.Should().Contain("href=\"/api/resend?FirstName=Joe%20Bloggs\"");
    }

    [TestMethod]
    public void ToHtmlList_ShouldEscapeLabelsAndValues()
    {
        dynamic row = new ExpandoObject();
        ((IDictionary<string, object?>)row)["<Unsafe>"] = "<script>alert(1)</script>";

        var html = ListFormatter.ToHtmlList(new[] { row });

        html.Should().Contain("&lt;Unsafe&gt;");
        html.Should().Contain("&lt;script&gt;alert(1)&lt;/script&gt;");
        html.Should().NotContain("<script>alert(1)</script>");
    }

    [TestMethod]
    public void ToHtmlList_WithMultipleRecords_ShouldRejectAmbiguousOutput()
    {
        dynamic first = new ExpandoObject();
        first.Name = "First";
        dynamic second = new ExpandoObject();
        second.Name = "Second";

        var act = () => ListFormatter.ToHtmlList(new[] { first, second });

        act.Should().Throw<ArgumentException>()
            .WithMessage("*exactly one record*received 2*");
    }
}
