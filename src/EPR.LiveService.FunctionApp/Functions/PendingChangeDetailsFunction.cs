using System.Net;
using Dapper;
using EPR.LiveService.FunctionApp.Formatting;
using EPR.LiveService.FunctionApp.PendingChanges;
using EPR.LiveService.FunctionApp.Services;
using EPR.LiveService.FunctionApp.Sql;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace EPR.LiveService.FunctionApp.Functions;

public class PendingChangeDetailsFunction(
    ISqlConnectionFactory connectionFactory,
    IOrganisationService organisationService)
    {
    [Function("PendingChangeDetailsForm")]
    [AuthorizeFunction(Roles.Admin)]
    public static async Task<HttpResponseData> ShowForm(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pending-change-details")] HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/html; charset=utf-8");
        await response.WriteStringAsync(PendingChangeDetailsPage.Build(new PendingChangeDetailsRequest
        {
            RegulatorEmail = req.Query.Get(nameof(PendingChangeDetailsRequest.RegulatorEmail)),
            UserEmail = req.Query.Get(nameof(PendingChangeDetailsRequest.UserEmail)),
            UserOrganisationId = req.Query.Get(nameof(PendingChangeDetailsRequest.UserOrganisationId))
        }));
        return response;
    }

    [Function("UpdatePendingChangeDetails")]
    [AuthorizeFunction(Roles.Admin)]
    public async Task<HttpResponseData> RunQuery(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "update-pending-change-details")]
            HttpRequestData req, CancellationToken cancellationToken)
    {
        var pendingChangeDetailsRequest = await req.ReadFromJsonAsync<PendingChangeDetailsRequest>();
        if (pendingChangeDetailsRequest is null)
        {
            return await WriteJsonAsync(
                req.CreateResponse(HttpStatusCode.BadRequest),
                new { error = "A JSON request body is required." });
        }

        var errors = pendingChangeDetailsRequest.Validate();
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
        var pendingChangeRegulatorDetails = await connection.QueryFirstOrDefaultAsync<PendingChangeRegulatorDetails>(regulatorDetailsSql, pendingChangeDetailsRequest);

        if (pendingChangeRegulatorDetails is null)
        {
            return await WriteJsonAsync(
                req.CreateResponse(HttpStatusCode.NotFound),
                new { error = "No matching regulator or pending change history was found." });
        }

        var updateOrganisationResult = await organisationService.UpdateOrganisationAsync(
            pendingChangeRegulatorDetails,
            pendingChangeDetailsRequest.RegulatorResponse!.Equals(
                "Accepted",
                StringComparison.OrdinalIgnoreCase),
            pendingChangeDetailsRequest.RegulatorComments ?? string.Empty,
            pendingChangeDetailsRequest.BearerToken!,
            cancellationToken);

        return await WriteJsonAsync(
            req.CreateResponse(HttpStatusCode.OK),
            new
            {
                pendingChangeRegulatorDetails.XEprUser,
                pendingChangeRegulatorDetails.XEprOrganisation,
                pendingChangeRegulatorDetails.ChangeHistoryExternalId,
                UpdateOrganisationResult = updateOrganisationResult
            });
    }

    private static async Task<HttpResponseData> WriteJsonAsync(HttpResponseData response, object value)
    {
        await response.WriteAsJsonAsync(value);
        return response;
    }
}
