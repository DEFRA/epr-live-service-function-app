using EPR.LiveService.FunctionApp.Notifications;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
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
        var uri = new Uri($"https://localhost{action.Url}");
        var query = QueryHelpers.ParseQuery(uri.Query);

        action.Label.Should().Be("Re-send invitation email");
        uri.AbsolutePath.Should().Be("/api/resend-invite-email");
        query["EmailAddress"].Should().ContainSingle("joe+invite@example.com");
        query["OrganisationName"].Should().ContainSingle("Kell & Bloggs");
        query["FirstName"].Should().ContainSingle("Joe");
        query["LastName"].Should().ContainSingle("Bloggs");
        query["JoinTheTeamLink"].Should().ContainSingle("https://example.com/join?a=1&b=2");
    }

    [TestMethod]
    public void InvitationDetails_ShouldAllowEveryMappedFieldToBeMissing()
    {
        var action = _provider.GetActions(
                "invitation_details",
                new Dictionary<string, object> { ["UnrelatedField"] = "value" })
            .Should().ContainSingle().Subject;

        action.Url.Should().Be("/api/resend-invite-email");
    }

    [TestMethod]
    public void OtherQueries_ShouldNotReceiveResendAction()
    {
        _provider.GetActions("organisation_details", new Dictionary<string, object>())
            .Should().BeEmpty();
    }
}
