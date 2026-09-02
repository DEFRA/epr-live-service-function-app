using EPR.LiveService.FunctionApp.Notifications;
using FluentAssertions;
using EPR.LiveService.FunctionApp.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Notifications;

[TestClass]
public class InvitationResendActionProviderTests
{
    private readonly ResendInvitateEmailActionProvider _provider = new();

    [TestMethod]
    public void InvitationDetails_ShouldMapAvailableFieldsToResendParameters()
    {
        var record = new Dictionary<string, object>
        {
            ["InvitedUserEmail"] = "joe+invite@example.com",
            ["OrganisationName"] = "Kell & Bloggs",
            ["FirstName"] = "Joe",
            ["LastName"] = "Bloggs",
            ["InviteLink"] = "https://example.com/join?a=1&b=2"
        };

        var action = _provider.GetActions("invitation_details", record)
            .Should().ContainSingle().Subject;
        
        action.Label.Should().Be("Re-send invitation email");
        action.Url.Should().Be("/api/resend-invite-email");
        action.Fields.Should().BeEquivalentTo(
        [
            new QueryResultActionField("EmailAddress", "joe+invite@example.com"),
            new QueryResultActionField("OrganisationName", "Kell & Bloggs"),
            new QueryResultActionField("FirstName", "Joe"),
            new QueryResultActionField("LastName", "Bloggs"),
            new QueryResultActionField("JoinTheTeamLink", "https://example.com/join?a=1&b=2")
        ]);
    }

    [TestMethod]
    public void InvitationDetails_ShouldAllowEveryMappedFieldToBeMissing()
    {
        var action = _provider.GetActions(
                "invitation_details",
                new Dictionary<string, object> { ["UnrelatedField"] = "value" })
            .Should().ContainSingle().Subject;

        action.Url.Should().Be("/api/resend-invite-email");
        action.Fields.Should().BeEmpty();
    }

    [TestMethod]
    public void OtherQueries_ShouldNotReceiveResendAction()
    {
        _provider.GetActions("organisation_details", new Dictionary<string, object>())
            .Should().BeEmpty();
    }
}
