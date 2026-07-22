using EPR.LiveService.FunctionApp.Formatting;
using EPR.LiveService.FunctionApp.Functions;
using EPR.LiveService.FunctionApp.Notifications;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Notifications;

[TestClass]
public class ResendInviteEmailTests
{
    [TestMethod]
    public void ValidRequest_ShouldMapExpectedNotifyPersonalisation()
    {
        var request = ValidRequest();

        request.Validate().Should().BeEmpty();
        request.ToPersonalisation().Should().BeEquivalentTo(new Dictionary<string, dynamic>
        {
            ["OrganisationName"] = "Kellbloggs",
            ["FirstName"] = "Joe",
            ["LastName"] = "Bloggs",
            ["JoinTheTeamLink"] = "https://example.com/join/abc"
        });
        request.ToPersonalisation().Should().NotContainKey("EmailAddress");
    }

    [TestMethod]
    public void InvalidRequest_ShouldReportMissingAndMalformedValues()
    {
        var request = new ResendInviteEmailRequest
        {
            EmailAddress = "not-an-email",
            OrganisationName = " ",
            FirstName = null,
            LastName = "",
            JoinTheTeamLink = "not-a-url"
        };

        request.Validate().Should().BeEquivalentTo(
            "OrganisationName is required.",
            "FirstName is required.",
            "LastName is required.",
            "EmailAddress must be a valid email address.",
            "JoinTheTeamLink must be an absolute HTTP or HTTPS URL.");
    }

    [TestMethod]
    public void Form_ShouldCollectEveryRequiredParameter()
    {
        var html = ResendInviteEmailPage.Build();

        html.Should().Contain("Re-send Extended Producer Responsibility for Packaging");
        html.Should().Contain("name=\"EmailAddress\"").And.Contain("required");
        html.Should().Contain("name=\"OrganisationName\"");
        html.Should().Contain("name=\"FirstName\"");
        html.Should().Contain("name=\"LastName\"");
        html.Should().Contain("name=\"JoinTheTeamLink\"");
        html.Should().Contain("fetch('/api/resend-invite-email'");
    }

    [TestMethod]
    public void Form_ShouldPrefillAnyProvidedFieldsAndEscapeThem()
    {
        var html = ResendInviteEmailPage.Build(new ResendInviteEmailRequest
        {
            EmailAddress = "joe@example.com",
            OrganisationName = "Kell & <Bloggs>"
        });

        html.Should().Contain("value=\"joe@example.com\"");
        html.Should().Contain("value=\"Kell &amp; &lt;Bloggs&gt;\"");
        html.Should().Contain("id=\"FirstName\" name=\"FirstName\" value=\"\"");
    }

    [TestMethod]
    public void Feature_ShouldUseProvidedTemplateId()
    {
        ResendInviteEmailFunction.TemplateId.Should()
            .Be("958280bf-e77e-4940-ba37-74340c02e44d");
    }

    private static ResendInviteEmailRequest ValidRequest() => new()
    {
        EmailAddress = "joe.bloggs@example.com",
        OrganisationName = "Kellbloggs",
        FirstName = "Joe",
        LastName = "Bloggs",
        JoinTheTeamLink = "https://example.com/join/abc"
    };
}
