using EPR.LiveService.FunctionApp.Formatting;
using EPR.LiveService.FunctionApp.UserDetailsChange;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.UserDetailsChange;

[TestClass]
public class UserDetailsChangeTests
{
    [TestMethod]
    public void ValidRequest_ShouldHaveNoValidationErrors()
    {
        ValidRequest().Validate().Should().BeEmpty();
    }

    [TestMethod]
    public void InvalidRequest_ShouldReportMissingAndMalformedValues()
    {
        var request = new UserDetailsChangeRequest
        {
            RegulatorEmail = "not-an-email",
            UserEmail = null,
            UserOrganisationId = " ",
            RegulatorResponse = "Deferred"
        };

        request.Validate().Should().BeEquivalentTo(
            "UserEmail is required.",
            "UserOrganisationId is required.",
            "RegulatorEmail must be a valid email address.",
            "RegulatorResponse must be either Accepted or Rejected.");
    }

    [TestMethod]
    public void RejectedRequest_ShouldRequireRegulatorComments()
    {
        var request = ValidRequest();
        request.RegulatorResponse = "Rejected";

        request.Validate().Should().ContainSingle()
            .Which.Should().Be("RegulatorComments is required when RegulatorResponse is Rejected.");

        request.RegulatorComments = "The evidence was insufficient.";
        request.Validate().Should().BeEmpty();
    }

    [TestMethod]
    public void Form_ShouldCollectEveryRequiredParameter()
    {
        var html = UserDetailsChangePage.Build();

        html.Should().Contain("Update User Details");
        html.Should().Contain("name=\"RegulatorEmail\"").And.Contain("required");
        html.Should().Contain("name=\"UserEmail\"");
        html.Should().Contain("name=\"UserOrganisationId\"");
        html.Should().Contain("name=\"RegulatorResponse\" value=\"Accepted\"");
        html.Should().Contain("name=\"RegulatorResponse\" value=\"Rejected\"");
        html.Should().Contain("name=\"RegulatorComments\"");
        html.Should().Contain("fetch('/api/update-user-details'");
    }

    [TestMethod]
    public void Form_ShouldPrefillProvidedFieldsAndEscapeThem()
    {
        var html = UserDetailsChangePage.Build(new UserDetailsChangeRequest
        {
            RegulatorEmail = "regulator@example.com",
            UserOrganisationId = "ORG<&>",
            RegulatorResponse = "Rejected",
            RegulatorComments = "Reason <&>"
        });

        html.Should().Contain("value=\"regulator@example.com\"");
        html.Should().Contain("value=\"ORG&lt;&amp;&gt;\"");
        html.Should().Contain("value=\"Rejected\" checked");
        html.Should().Contain("Reason &lt;&amp;&gt;");
        html.Should().Contain("id=\"UserEmail\" name=\"UserEmail\" value=\"\"");
    }

    private static UserDetailsChangeRequest ValidRequest() => new()
    {
        RegulatorEmail = "regulator@example.com",
        UserEmail = "user@example.com",
        UserOrganisationId = "123456",
        RegulatorResponse = "Accepted"
    };
}
