using System.Net;
using EPR.LiveService.FunctionApp.Functions;
using EPR.LiveService.FunctionApp.UnitTests.TestSupport.Fakes;
using EPR.LiveService.FunctionApp.UnitTests.TestSupport.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Functions;

[TestClass]
public class UserDetailsChangeFunctionTests
{
    private static UserDetailsChangeFunction CreateFunction() =>
        new(
            new UnreachableSqlConnectionFactory(),
            new UnreachableOrganisationService());

    [TestMethod]
    public async Task ShowForm_ShouldPrefillFieldsFromQueryString()
    {
        var request = TestHttpRequestData.Get(
            "/api/user-details-change",
            new Dictionary<string, string>
            {
                ["RegulatorEmail"] = "regulator@example.gov.uk",
                ["UserEmail"] = "user@example.com",
                ["UserOrganisationId"] = "123456"
            });

        var response = await UserDetailsChangeFunction.ShowForm(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = response.ReadBodyAsString();
        html.Should().Contain("regulator@example.gov.uk");
        html.Should().Contain("user@example.com");
        html.Should().Contain("123456");
    }

    [TestMethod]
    public async Task RunQuery_WithMissingBody_ShouldReturnBadRequest()
    {
        var function = CreateFunction();
        var request = TestHttpRequestData.PostJson("/api/update-user-details", "null");

        var response = await function.RunQuery(request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.ReadBodyAsString().Should().Contain("A JSON request body is required.");
    }

    [TestMethod]
    public async Task RunQuery_WithInvalidRequest_ShouldReturnValidationErrors()
    {
        var function = CreateFunction();
        var request = TestHttpRequestData.PostJson(
            "/api/update-user-details",
            """{ "regulatorEmail": "not-an-email" }""");

        var response = await function.RunQuery(request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = response.ReadBodyAsString();
        body.Should().Contain("UserEmail is required");
        body.Should().Contain("must be a valid email address");
    }

    [TestMethod]
    public async Task RunQuery_WhenRejectedWithoutComments_ShouldReturnValidationError()
    {
        var function = CreateFunction();
        var request = TestHttpRequestData.PostJson(
            "/api/update-user-details",
            """
            {
                "regulatorEmail": "regulator@example.gov.uk",
                "userEmail": "user@example.com",
                "userOrganisationId": "123456",
                "regulatorResponse": "Rejected"
            }
            """);

        var response = await function.RunQuery(request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.ReadBodyAsString().Should().Contain("RegulatorComments is required when RegulatorResponse is Rejected");
    }
}
