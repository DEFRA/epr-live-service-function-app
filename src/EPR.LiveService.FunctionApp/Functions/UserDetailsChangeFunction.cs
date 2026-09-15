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

    private static readonly string RegulatorDetailsSql = LoadEmbeddedSql("RegulatorDetails.sql");

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

        using var connection = await connectionFactory.CreateConnectionAsync("accounts");
        var regulatorDetails = await connection.QueryFirstOrDefaultAsync<RegulatorDetails>(RegulatorDetailsSql, userDetailsChangeRequest);

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

    private static string LoadEmbeddedSql(string fileName)
    {
        var assembly = typeof(UserDetailsChangeFunction).Assembly;
        var resourceName = $"{assembly.GetName().Name}.UserDetailsChange.Sql.{fileName}";
    
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"SQL resource '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static async Task<HttpResponseData> WriteJsonAsync(HttpResponseData response, object value)
    {
        await response.WriteAsJsonAsync(value);
        return response;
    }
}
