using System.Net;
using EPR.LiveService.FunctionApp.Formatting;
using EPR.LiveService.FunctionApp.Notifications;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace EPR.LiveService.FunctionApp.Functions;

public class ResendInviteEmailFunction(IEmailNotificationSender sender)
{
    public const string TemplateId = "958280bf-e77e-4940-ba37-74340c02e44d";

    [Function("ResendInviteForm")]
    [AuthorizeFunction(Roles.Admin)]
    public static async Task<HttpResponseData> ShowForm(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "resend-invite-email")] HttpRequestData req)
    {
        var values = string.Equals(req.Method, "POST", StringComparison.OrdinalIgnoreCase)
            ? await ReadFormValuesAsync(req)
            : [];
    
        string? Get(string key) => values.TryGetValue(key, out var v) ? v.ToString() : null;
    
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/html; charset=utf-8");
        await response.WriteStringAsync(ResendInviteEmailPage.Build(new ResendInviteEmailRequest
        {
            EmailAddress = Get(nameof(ResendInviteEmailRequest.EmailAddress)),
            OrganisationName = Get(nameof(ResendInviteEmailRequest.OrganisationName)),
            FirstName = Get(nameof(ResendInviteEmailRequest.FirstName)),
            LastName = Get(nameof(ResendInviteEmailRequest.LastName)),
            JoinTheTeamLink = Get(nameof(ResendInviteEmailRequest.JoinTheTeamLink))
        }));
        return response;
    }

    [Function("ResendInvite")]
    [AuthorizeFunction(
        Roles.Admin)]
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

    private static async Task<Dictionary<string, StringValues>> ReadFormValuesAsync(HttpRequestData req)
    {
        using var reader = new StreamReader(req.Body);
        var body = await reader.ReadToEndAsync();
        return QueryHelpers.ParseQuery(body); // same encoding as querystrings, works for x-www-form-urlencoded bodies
    }

    private static async Task<HttpResponseData> WriteJsonAsync(HttpResponseData response, object value)
    {
        await response.WriteAsJsonAsync(value);
        return response;
    }
}
