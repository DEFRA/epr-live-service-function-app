using EPR.LiveService.FunctionApp.Formatting;
using EPR.LiveService.FunctionApp.PendingChanges;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.PendingChanges;

[TestClass]
public class PendingChangeDetailsTests
{
    [TestMethod]
    public void ValidRequest_ShouldHaveNoValidationErrors()
    {
        ValidRequest().Validate().Should().BeEmpty();
    }

    [TestMethod]
    public void InvalidRequest_ShouldReportMissingAndMalformedValues()
    {
        var request = new PendingChangeDetailsRequest
        {
            BearerToken = null,
            RegulatorEmail = "not-an-email",
            UserEmail = null,
            UserOrganisationId = " "
        };

        request.Validate().Should().BeEquivalentTo(
            "BearerToken is required.",
            "UserEmail is required.",
            "UserOrganisationId is required.",
            "RegulatorEmail must be a valid email address.");
    }

    [TestMethod]
    public void Form_ShouldCollectEveryRequiredParameter()
    {
        var html = PendingChangeDetailsPage.Build();

        html.Should().Contain("Pending Change Details");
        html.Should().Contain("type=\"password\" id=\"BearerToken\" name=\"BearerToken\"")
            .And.Contain("autocomplete=\"off\"");
        html.Should().Contain("name=\"RegulatorEmail\"").And.Contain("required");
        html.Should().Contain("name=\"UserEmail\"");
        html.Should().Contain("name=\"UserOrganisationId\"");
        html.Should().Contain("fetch('/api/pending-change-details'");
    }

    [TestMethod]
    public void Form_ShouldPrefillProvidedFieldsAndEscapeThem()
    {
        var html = PendingChangeDetailsPage.Build(new PendingChangeDetailsRequest
        {
            BearerToken = "token<&>",
            RegulatorEmail = "regulator@example.com",
            UserOrganisationId = "ORG<&>"
        });

        html.Should().Contain("value=\"token&lt;&amp;&gt;\"");
        html.Should().Contain("value=\"regulator@example.com\"");
        html.Should().Contain("value=\"ORG&lt;&amp;&gt;\"");
        html.Should().Contain("id=\"UserEmail\" name=\"UserEmail\" value=\"\"");
    }

    private static PendingChangeDetailsRequest ValidRequest() => new()
    {
        BearerToken = "test-token",
        RegulatorEmail = "regulator@example.com",
        UserEmail = "user@example.com",
        UserOrganisationId = "123456"
    };
}
