using System.Net;
using EPR.LiveService.FunctionApp.Functions;
using EPR.LiveService.FunctionApp.Notifications;
using EPR.LiveService.FunctionApp.UnitTests.TestSupport.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Functions;

[TestClass]
public class ResendInviteEmailFunctionTests
{
    [TestMethod]
    public async Task Send_WithMissingBody_ShouldReturnBadRequestWithoutSendingEmail()
    {
        var sender = new FakeEmailNotificationSender();
        var function = new ResendInviteEmailFunction(sender);
        var request = TestHttpRequestData.PostJson("/api/resend-invite-email/send", "null");

        var response = await function.Send(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.ReadBodyAsString().Should().Contain("A JSON request body is required.");
        sender.SentEmails.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Send_WithInvalidRequest_ShouldReturnValidationErrorsWithoutSendingEmail()
    {
        var sender = new FakeEmailNotificationSender();
        var function = new ResendInviteEmailFunction(sender);
        var request = TestHttpRequestData.PostJson(
            "/api/resend-invite-email/send",
            """{ "emailAddress": "not-an-email" }""");

        var response = await function.Send(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.ReadBodyAsString().Should().Contain("must be a valid email address");
        sender.SentEmails.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Send_WithValidRequest_ShouldSendEmailAndReturnConfirmation()
    {
        var sender = new FakeEmailNotificationSender();
        var function = new ResendInviteEmailFunction(sender);
        var request = TestHttpRequestData.PostJson(
            "/api/resend-invite-email/send",
            """
            {
                "emailAddress": "joe.bloggs@company.com",
                "organisationName": "Kellbloggs",
                "firstName": "Joe",
                "lastName": "Bloggs",
                "joinTheTeamLink": "https://example.gov.uk/join"
            }
            """);

        var response = await function.Send(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ReadBodyAsString().Should().Contain("joe.bloggs@company.com");
        sender.SentEmails.Should().ContainSingle(email =>
            email.EmailAddress == "joe.bloggs@company.com"
            && email.TemplateId == ResendInviteEmailFunction.TemplateId);
    }

    [TestMethod]
    public async Task ShowForm_WithGetRequest_ShouldRenderEmptyForm()
    {
        var request = TestHttpRequestData.Get("/api/resend-invite-email");
    
        var response = await ResendInviteEmailFunction.ShowForm(request);
    
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ReadBodyAsString().Should().Contain("<form");
    }
    
    [TestMethod]
    public async Task ShowForm_WithPostRequest_ShouldPrefillFieldsFromFormBody()
    {
        var request = TestHttpRequestData.PostForm(
            "/api/resend-invite-email",
            "EmailAddress=joe.bloggs%40company.com&OrganisationName=Kellbloggs&FirstName=Joe&LastName=Bloggs&JoinTheTeamLink=https%3A%2F%2Fexample.gov.uk%2Fjoin");
    
        var response = await ResendInviteEmailFunction.ShowForm(request);
    
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = response.ReadBodyAsString();
        html.Should().Contain("joe.bloggs@company.com");
        html.Should().Contain("Kellbloggs");
    }
}

file sealed class FakeEmailNotificationSender : IEmailNotificationSender
{
    public List<(string EmailAddress, string TemplateId, Dictionary<string, dynamic> Personalisation)> SentEmails { get; } = new();

    public Task SendAsync(string emailAddress, string templateId, Dictionary<string, dynamic> personalisation)
    {
        SentEmails.Add((emailAddress, templateId, personalisation));
        return Task.CompletedTask;
    }
}
