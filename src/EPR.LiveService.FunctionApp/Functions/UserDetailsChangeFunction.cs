using System.Net;
using Dapper;
using EPR.LiveService.FunctionApp.Formatting;
using EPR.LiveService.FunctionApp.UserDetailsChange;
using EPR.LiveService.FunctionApp.Services;
using EPR.LiveService.FunctionApp.Sql;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace EPR.LiveService.FunctionApp.Functions;

public class UserDetailsChangeFunction(
    ISqlConnectionFactory connectionFactory,
    IOrganisationService organisationService)
    {

    [Function("UserDetailsChangeForm")]
    [AuthorizeFunction(Roles.Admin)]
    public static async Task<HttpResponseData> ShowForm(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user-details-change")] HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/html; charset=utf-8");
        await response.WriteStringAsync(UserDetailsChangePage.Build(new UserDetailsChangeRequest
        {
            RegulatorEmail = req.Query.Get(nameof(UserDetailsChangeRequest.RegulatorEmail)),
            UserEmail = req.Query.Get(nameof(UserDetailsChangeRequest.UserEmail)),
            UserOrganisationId = req.Query.Get(nameof(UserDetailsChangeRequest.UserOrganisationId))
        }));
        return response;
    }

    [Function("UserDetailsUpdate")]
    [AuthorizeFunction(Roles.Admin)]
    public async Task<HttpResponseData> RunQuery(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "update-user-details")]
            HttpRequestData req, CancellationToken cancellationToken)
    {
        using var diagClient = new HttpClient();
        
        diagClient.DefaultRequestHeaders.Add("X-IDENTITY-HEADER", Environment.GetEnvironmentVariable("IDENTITY_HEADER"));
        var diagResponse = await diagClient.GetAsync($"{Environment.GetEnvironmentVariable("IDENTITY_ENDPOINT")}?api-version=2019-08-01&resource=api://1755c3c9-8ecb-4903-a61b-cc5cd81ec320");
        var diagBody = await diagResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"[DIAG] MSI raw response: {(int)diagResponse.StatusCode} {diagBody}");
        
        var userDetailsChangeRequest = await req.ReadFromJsonAsync<UserDetailsChangeRequest>();
        if (userDetailsChangeRequest is null)
        {
            return await WriteJsonAsync(
                req.CreateResponse(HttpStatusCode.BadRequest),
                new { error = "A JSON request body is required." });
        }

        var errors = userDetailsChangeRequest.Validate();
        if (errors.Count > 0)
        {
            return await WriteJsonAsync(
                req.CreateResponse(HttpStatusCode.BadRequest),
                new { errors });
        }

        string regulatorDetailsSql = """
            WITH RegulatorDetails AS
            (
                SELECT
                    u.UserId AS XEprUser,
                    o.ExternalId AS XEprOrganisation
                FROM dbo.Users u
                INNER JOIN dbo.Persons p ON p.UserId = u.Id
                INNER JOIN dbo.PersonOrganisationConnections poc ON poc.PersonId = p.Id
                INNER JOIN dbo.Organisations o ON o.Id = poc.OrganisationId
                WHERE u.Email = @RegulatorEmail
                  AND u.IsDeleted = 0
                  AND o.IsDeleted = 0
                  AND poc.IsDeleted = 0
            ),
            LatestChangeHistory AS
            (
                SELECT TOP (1)
                    ch.ExternalId AS ChangeHistoryExternalId
                FROM dbo.Users u
                INNER JOIN dbo.Persons p ON p.UserId = u.Id
                INNER JOIN dbo.PersonOrganisationConnections poc ON poc.PersonId = p.Id
                INNER JOIN dbo.Organisations o ON o.Id = poc.OrganisationId
                INNER JOIN dbo.ChangeHistory ch
                    ON ch.PersonId = p.Id
                    AND ch.OrganisationId = o.Id
                WHERE u.Email = @UserEmail
                  AND o.ReferenceNumber = @UserOrganisationId
                  AND ch.IsActive = 1
                  AND ch.DecisionDate IS NULL
                  AND ch.IsDeleted = 0
                  AND o.IsDeleted = 0
                  AND poc.IsDeleted = 0
                ORDER BY ch.DeclarationDate DESC
            )
            SELECT
                regulator.XEprUser,
                regulator.XEprOrganisation,
                changeHistory.ChangeHistoryExternalId
            FROM RegulatorDetails regulator
            CROSS JOIN LatestChangeHistory changeHistory;
            """;

        using var connection = await connectionFactory.CreateConnectionAsync("accounts");
        var regulatorDetails = await connection.QueryFirstOrDefaultAsync<RegulatorDetails>(regulatorDetailsSql, userDetailsChangeRequest);

        if (regulatorDetails is null)
        {
            return await WriteJsonAsync(
                req.CreateResponse(HttpStatusCode.NotFound),
                new { error = "No matching regulator or user details change history was found." });
        }

        var updateOrganisationResult = await organisationService.UpdateOrganisationAsync(
            regulatorDetails,
            userDetailsChangeRequest.RegulatorResponse!.Equals(
                "Accepted",
                StringComparison.OrdinalIgnoreCase),
            userDetailsChangeRequest.RegulatorComments ?? string.Empty,
            cancellationToken);

        return await WriteJsonAsync(
            req.CreateResponse(HttpStatusCode.OK),
            new
            {
                regulatorDetails.XEprUser,
                regulatorDetails.XEprOrganisation,
                regulatorDetails.ChangeHistoryExternalId,
                UpdateOrganisationResult = updateOrganisationResult
            });
    }

    private static async Task<HttpResponseData> WriteJsonAsync(HttpResponseData response, object value)
    {
        await response.WriteAsJsonAsync(value);
        return response;
    }
}
