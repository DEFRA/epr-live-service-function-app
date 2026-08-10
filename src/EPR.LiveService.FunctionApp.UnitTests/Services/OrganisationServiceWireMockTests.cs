using System.Net;
using EPR.LiveService.FunctionApp.Configs;
using EPR.LiveService.FunctionApp.PendingChanges;
using EPR.LiveService.FunctionApp.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace EPR.LiveService.FunctionApp.UnitTests.Services;

[TestClass]
public class OrganisationServiceWireMockTests
{
    private const string ApprovalEndpoint = "api/regulators/regulator-organisation/approval/";
    private const string BearerToken = "wiremock-bearer-token";

    [TestMethod]
    public async Task UpdateOrganisationAsync_WhenRequestIsAccepted_ReturnsAcceptedResponse()
    {
        using var server = WireMockServer.Start();
        var pendingChangeRegulatorDetails = CreatePendingChangeRegulatorDetails();
        const string regulatorComment = "";
        var request = CreateRequest(pendingChangeRegulatorDetails, true, regulatorComment);
        server
            .Given(request)
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new
                {
                    changeHistory = new { approverComments = regulatorComment },
                    hasUserDetailsChangeAccepted = true,
                    hasUserDetailsChangeRejected = false
                }));

        using var httpClient = CreateHttpClient(server);
        var result = await CreateService(httpClient).UpdateOrganisationAsync(
            pendingChangeRegulatorDetails,
            true,
            regulatorComment,
            BearerToken);

        result.HasUserDetailsChangeAccepted.Should().BeTrue();
        result.HasUserDetailsChangeRejected.Should().BeFalse();
        result.ChangeHistory.ApproverComments.Should().Be(regulatorComment);
        server.FindLogEntries(request).Should().ContainSingle();
    }

    [TestMethod]
    public async Task UpdateOrganisationAsync_WhenRequestIsRejected_ReturnsRejectedResponse()
    {
        using var server = WireMockServer.Start();
        var pendingChangeRegulatorDetails = CreatePendingChangeRegulatorDetails();
        const string regulatorComment = "requires change of AP request";
        var request = CreateRequest(pendingChangeRegulatorDetails, false, regulatorComment);
        server
            .Given(request)
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(RejectedResponseJson));

        using var httpClient = CreateHttpClient(server);
        var result = await CreateService(httpClient).UpdateOrganisationAsync(
            pendingChangeRegulatorDetails,
            false,
            regulatorComment,
            BearerToken);

        result.HasUserDetailsChangeAccepted.Should().BeFalse();
        result.HasUserDetailsChangeRejected.Should().BeTrue();
        result.ChangeHistory.ApprovedById.Should().Be(13835);
        result.ChangeHistory.ApproverComments.Should().Be(regulatorComment);
        result.ChangeHistory.ExternalId.Should().Be(
            Guid.Parse("70382b98-8cc5-49fa-8117-bfc89d4770e1"));
        result.ChangeHistory.OrganisationName.Should().Be("ACME Widgets (UK) Limited");
        result.ChangeHistory.BusinessAddress.Postcode.Should().Be("HT1 1TT");
        result.ChangeHistory.NewValues.FirstName.Should().Be("John");
        result.ChangeHistory.OldValues.FirstName.Should().Be("Jane");
        server.FindLogEntries(request).Should().ContainSingle();
    }

    [DataTestMethod]
    [DataRow(400, "The organisation service rejected the request.")]
    [DataRow(401, "The organisation service did not accept the supplied credentials.")]
    [DataRow(403, "The organisation service denied access to the requested operation.")]
    [DataRow(404, "The organisation or pending change was not found.")]
    [DataRow(409, "The organisation update conflicts with its current state.")]
    [DataRow(429, "The organisation service rate limit was exceeded.")]
    [DataRow(500, "The organisation service encountered an error.")]
    [DataRow(418, "The organisation service request was unsuccessful.")]
    public async Task UpdateOrganisationAsync_WhenEndpointReturnsError_ThrowsMappedException(
        int statusCode,
        string expectedMessage)
    {
        using var server = WireMockServer.Start();
        var pendingChangeRegulatorDetails = CreatePendingChangeRegulatorDetails();
        const string regulatorComment = "WireMock error scenario";
        var request = CreateRequest(pendingChangeRegulatorDetails, true, regulatorComment);
        server
            .Given(request)
            .RespondWith(Response.Create()
                .WithStatusCode(statusCode)
                .WithBodyAsJson(new { error = "Endpoint error" }));

        using var httpClient = CreateHttpClient(server);
        var action = () => CreateService(httpClient).UpdateOrganisationAsync(
            pendingChangeRegulatorDetails,
            true,
            regulatorComment,
            BearerToken);

        var exception = await action.Should().ThrowAsync<OrganisationServiceException>();
        exception.Which.StatusCode.Should().Be((HttpStatusCode)statusCode);
        exception.Which.Message.Should().Be(expectedMessage);
        server.FindLogEntries(request).Should().ContainSingle();
    }

    [TestMethod]
    public async Task UpdateOrganisationAsync_WhenEndpointReturnsMalformedJson_ThrowsInvalidResponseException()
    {
        using var server = WireMockServer.Start();
        var pendingChangeRegulatorDetails = CreatePendingChangeRegulatorDetails();
        const string regulatorComment = "Malformed response scenario";
        var request = CreateRequest(pendingChangeRegulatorDetails, true, regulatorComment);
        server
            .Given(request)
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody("not-json"));

        await VerifyInvalidResponseAsync(server, request, pendingChangeRegulatorDetails, regulatorComment);
    }

    [TestMethod]
    public async Task UpdateOrganisationAsync_WhenEndpointReturnsEmptyBody_ThrowsInvalidResponseException()
    {
        using var server = WireMockServer.Start();
        var pendingChangeRegulatorDetails = CreatePendingChangeRegulatorDetails();
        const string regulatorComment = "Empty response scenario";
        var request = CreateRequest(pendingChangeRegulatorDetails, true, regulatorComment);
        server
            .Given(request)
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK));

        await VerifyInvalidResponseAsync(server, request, pendingChangeRegulatorDetails, regulatorComment);
    }

    private static async Task VerifyInvalidResponseAsync(
        WireMockServer server,
        IRequestBuilder request,
        PendingChangeRegulatorDetails pendingChangeRegulatorDetails,
        string regulatorComment)
    {
        using var httpClient = CreateHttpClient(server);
        var action = () => CreateService(httpClient).UpdateOrganisationAsync(
            pendingChangeRegulatorDetails,
            true,
            regulatorComment,
            BearerToken);

        var exception = await action.Should().ThrowAsync<OrganisationServiceException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.OK);
        exception.Which.Message.Should().Be(
            "The organisation service returned an invalid response.");
        server.FindLogEntries(request).Should().ContainSingle();
    }

    private static IRequestBuilder CreateRequest(
        PendingChangeRegulatorDetails pendingChangeRegulatorDetails,
        bool hasRegulatorAccepted,
        string regulatorComment) => Request.Create()
            .WithPath($"/{ApprovalEndpoint}{pendingChangeRegulatorDetails.ChangeHistoryExternalId}")
            .UsingPost()
            .WithHeader("Authorization", $"Bearer {BearerToken}")
            .WithHeader("X-EPR-User", pendingChangeRegulatorDetails.XEprUser.ToString())
            .WithHeader("X-EPR-Organisation", pendingChangeRegulatorDetails.XEprOrganisation.ToString())
            .WithBodyAsJson(new
            {
                regulatorComment,
                hasRegulatorAccepted
            });

    private static HttpClient CreateHttpClient(WireMockServer server) => new()
    {
        BaseAddress = new Uri(server.Url!)
    };

    private static OrganisationService CreateService(HttpClient httpClient) => new(
        httpClient,
        CreateApiEndpoint(),
        NullLogger<OrganisationService>.Instance);

    private static PendingChangeRegulatorDetails CreatePendingChangeRegulatorDetails() => new()
    {
        XEprUser = Guid.Parse("5dd870a9-5b91-4e66-9d4f-d9fc44e891d5"),
        XEprOrganisation = Guid.Parse("5a937d4d-6934-45f0-bafe-b2368ef46f41"),
        ChangeHistoryExternalId = Guid.Parse("6cd22e54-d800-48c2-b825-940d4218f035")
    };

    private static ApiEndpoint CreateApiEndpoint()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{ApiEndpoint.SectionName}:{nameof(ApiEndpoint.RegulatorOrganisationApproval)}"] =
                    ApprovalEndpoint
            })
            .Build();

        return ApiEndpoint.FromConfiguration(
            configuration.GetSection(ApiEndpoint.SectionName));
    }

    private const string RejectedResponseJson = """
        {
          "changeHistory": {
            "approvedById": 13835,
            "approverComments": "requires change of AP request",
            "businessAddress": {
              "buildingName": null,
              "buildingNumber": "70",
              "country": "England",
              "county": null,
              "dependentLocality": null,
              "locality": null,
              "postcode": "HT1 1TT",
              "street": "Main Street",
              "subBuildingName": null,
              "town": "High Town"
            },
            "companiesHouseNumber": "03043172",
            "createdOn": "2026-07-29T14:34:49.013002+00:00",
            "decisionDate": "2026-08-07T09:09:52.1964924+00:00",
            "declarationDate": "2026-07-29T14:34:49.007462+00:00",
            "emailAddress": "mickey.mouse@example.com",
            "externalId": "70382b98-8cc5-49fa-8117-bfc89d4770e1",
            "id": 425,
            "isActive": true,
            "lastUpdatedOn": "2026-08-07T09:09:52.2087504+00:00",
            "nation": "England",
            "newValues": {
              "firstName": "John",
              "jobTitle": "Director",
              "lastName": "Doe"
            },
            "oldValues": {
              "firstName": "Jane",
              "jobTitle": "Director",
              "lastName": "Doe"
            },
            "organisationId": 3363,
            "organisationName": "ACME Widgets (UK) Limited",
            "organisationReferenceNumber": "103363",
            "organisationType": "Companies House Company",
            "personId": 4740,
            "serviceRole": "Approved Person",
            "telephone": "01234567890"
          },
          "hasUserDetailsChangeAccepted": false,
          "hasUserDetailsChangeRejected": true
        }
        """;
}
