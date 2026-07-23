using System.Net;
using EPR.LiveService.FunctionApp.Formatting;
using EPR.LiveService.FunctionApp.Notifications;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace EPR.LiveService.FunctionApp.Functions;

public class ResendInviteEmailFunction(IEmailNotificationSender sender)
{
    public const string TemplateId = "958280bf-e77e-4940-ba37-74340c02e44d";

    [Function("ResendInviteForm")]
    public static async Task<HttpResponseData> ShowForm(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "resend-invite-email")] HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/html; charset=utf-8");
        await response.WriteStringAsync(ResendInviteEmailPage.Build(new ResendInviteEmailRequest
        {
            EmailAddress = req.Query.Get(nameof(ResendInviteEmailRequest.EmailAddress)),
            OrganisationName = req.Query.Get(nameof(ResendInviteEmailRequest.OrganisationName)),
            FirstName = req.Query.Get(nameof(ResendInviteEmailRequest.FirstName)),
            LastName = req.Query.Get(nameof(ResendInviteEmailRequest.LastName)),
            JoinTheTeamLink = req.Query.Get(nameof(ResendInviteEmailRequest.JoinTheTeamLink))
        }));
        return response;
    }

    [Function("ResendInvite")]
    public async Task<HttpResponseData> Send(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "resend-invite-email")] HttpRequestData req)
    {
        var request = await req.ReadFromJsonAsync<ResendInviteEmailRequest>();
        if (request is null)
        {
            return await WriteJsonAsync(
                req.CreateResponse(HttpStatusCode.BadRequest),
                new { error = "A JSON request body is required." });
        }

        var errors = request.Validate();
        if (errors.Count > 0)
        {
            return await WriteJsonAsync(
                req.CreateResponse(HttpStatusCode.BadRequest),
                new { errors });
        }

        await sender.SendAsync(
            request.EmailAddress!,
            TemplateId,
            request.ToPersonalisation());

        return await WriteJsonAsync(
            req.CreateResponse(HttpStatusCode.OK),
            new { message = $"Email sent to {request.EmailAddress}." });
    }

    private static async Task<HttpResponseData> WriteJsonAsync(HttpResponseData response, object value)
    {
        await response.WriteAsJsonAsync(value);
        return response;
    }
}
