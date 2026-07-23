using EPR.LiveService.FunctionApp.Queries;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Queries;

[TestClass]
public class InvitationDetailsQueryTests
{
    private readonly QueryRegistry _registry = new();

    [TestMethod]
    public void Definition_ShouldUseAccountsTargetAndRequireEmail()
    {
        var definition = _registry.Get("invitation_details");

        definition.DisplayName.Should().Be("Invitation Details");
        definition.Target.Should().Be("accounts");
        definition.Parameters.Should().ContainSingle();

        var parameter = definition.Parameters.Single();
        parameter.Name.Should().Be("Email");
        parameter.Label.Should().Be("Email address");
        parameter.Type.Should().Be("text");
        parameter.Required.Should().BeTrue();
    }

    [TestMethod]
    public async Task Script_ShouldUseParameterisedEmailAndContainExpectedInvitationFields()
    {
        var sql = await _registry.LoadScriptAsync("invitation_details");

        sql.Should().Contain("WHERE u.Email = @Email");
        sql.Should().NotContain("jordan.rowe+pre2_non-ch@equalexperts.com");
        sql.Should().Contain("AS TemplateLink");
        sql.Should().Contain("AS InviteLink");
        sql.Should().Contain("ORDER BY e.EnrolmentStatusId");
    }
}
