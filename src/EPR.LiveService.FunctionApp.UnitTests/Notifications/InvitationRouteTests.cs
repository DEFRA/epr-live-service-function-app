using System.Reflection;
using EPR.LiveService.FunctionApp.Functions;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Notifications;

[TestClass]
public class InvitationRouteTests
{
    [TestMethod]
    public void PrefillAndSend_ShouldHaveDistinctPostRoutes()
    {
        var form = Trigger(nameof(ResendInviteEmailFunction.ShowForm));
        var send = Trigger(nameof(ResendInviteEmailFunction.Send));

        form.Route.Should().Be("resend-invite-email");
        form.Methods.Should().BeEquivalentTo("get", "post");
        send.Route.Should().Be("resend-invite-email/send");
        send.Methods.Should().ContainSingle().Which.Should().Be("post");
    }

    private static HttpTriggerAttribute Trigger(string method) =>
        typeof(ResendInviteEmailFunction).GetMethod(method)!
            .GetParameters()[0].GetCustomAttribute<HttpTriggerAttribute>()!;
}
