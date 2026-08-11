using System.Net;
using System.Text;
using EPR.LiveService.FunctionApp.Configs;
using EPR.LiveService.FunctionApp.Services;
using EPR.LiveService.FunctionApp.UserDetailsChange;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Services;

[TestClass]
public class OrganisationServiceTests
{
    [TestMethod]
    public async Task UpdateOrganisationAsync_WhenRequestIsRejected_ThrowsExceptionWithStatusCode()
    {
        var service = CreateService((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Invalid request")
        }));

        var action = () => service.UpdateOrganisationAsync(CreateDetails(), true, string.Empty, "token");

        var exception = await action.Should().ThrowAsync<OrganisationServiceException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.Which.Message.Should().Be("The organisation service rejected the request.");
    }

    [TestMethod]
    public async Task UpdateOrganisationAsync_WhenRequestTimesOut_ThrowsOrganisationServiceException()
    {
        var service = CreateService((_, _) => throw new TaskCanceledException("Timed out"));

        var action = () => service.UpdateOrganisationAsync(CreateDetails(), true, string.Empty, "token");

        var exception = await action.Should().ThrowAsync<OrganisationServiceException>();
        exception.Which.Message.Should().Be("The organisation service request timed out.");
    }

    [TestMethod]
    public async Task UpdateOrganisationAsync_WhenResponseIsInvalid_ThrowsOrganisationServiceException()
    {
        var service = CreateService((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not-json", Encoding.UTF8, "application/json")
        }));

        var action = () => service.UpdateOrganisationAsync(CreateDetails(), true, string.Empty, "token");

        var exception = await action.Should().ThrowAsync<OrganisationServiceException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.OK);
        exception.Which.Message.Should().Be("The organisation service returned an invalid response.");
    }

    [TestMethod]
    public async Task UpdateOrganisationAsync_WhenCallerCancels_PreservesCancellation()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        var service = CreateService((_, cancellationToken) =>
            throw new OperationCanceledException(cancellationToken));

        var action = () => service.UpdateOrganisationAsync(
            CreateDetails(),
            true,
            string.Empty,
            "token",
            cancellationTokenSource.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    private static OrganisationService CreateService(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{ApiEndpoint.SectionName}:{nameof(ApiEndpoint.RegulatorOrganisationApproval)}"] =
                    "api/regulators/regulator-organisation/approval/"
            })
            .Build();
        var endpoints = ApiEndpoint.FromConfiguration(
            configuration.GetSection(ApiEndpoint.SectionName));
        var httpClient = new HttpClient(new StubHttpMessageHandler(sendAsync))
        {
            BaseAddress = new Uri("https://organisation-service.test/")
        };

        return new OrganisationService(
            httpClient,
            endpoints,
            NullLogger<OrganisationService>.Instance);
    }

    private static RegulatorDetails CreateDetails() => new()
    {
        XEprUser = Guid.NewGuid(),
        XEprOrganisation = Guid.NewGuid(),
        ChangeHistoryExternalId = Guid.NewGuid()
    };

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => sendAsync(request, cancellationToken);
    }
}
