using System.Net;
using System.Text;
using EPR.LiveService.FunctionApp.Configs;
using EPR.LiveService.FunctionApp.Services;
using EPR.LiveService.FunctionApp.UserDetailsChange;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

        var action = () => service.UpdateOrganisationAsync(CreateDetails(), true, string.Empty);

        var exception = await action.Should().ThrowAsync<OrganisationServiceException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.Which.Message.Should().Be("The organisation service rejected the request.");
    }

    [TestMethod]
    public async Task UpdateOrganisationAsync_WhenRequestTimesOut_ThrowsOrganisationServiceException()
    {
        var logger = new RecordingLogger<OrganisationService>();
        var service = CreateService(
            (_, _) => throw new TaskCanceledException("Timed out"),
            logger);

        var action = () => service.UpdateOrganisationAsync(CreateDetails(), true, string.Empty);

        var exception = await action.Should().ThrowAsync<OrganisationServiceException>();
        exception.Which.Message.Should().Be("The organisation service request timed out.");
        exception.Which.InnerException.Should().BeOfType<TaskCanceledException>();
        logger.Entries.Should().ContainSingle(entry =>
            entry.Level == LogLevel.Error &&
            entry.Exception == exception.Which);
    }

    [TestMethod]
    public async Task UpdateOrganisationAsync_WhenHttpRequestFails_PreservesStatusAndInnerException()
    {
        var requestException = new HttpRequestException(
            "Connection failed",
            null,
            HttpStatusCode.ServiceUnavailable);
        var service = CreateService((_, _) => throw requestException);

        var action = () => service.UpdateOrganisationAsync(CreateDetails(), true, string.Empty);

        var exception = await action.Should().ThrowAsync<OrganisationServiceException>();
        exception.Which.Message.Should().Be("The organisation service request could not be completed.");
        exception.Which.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        exception.Which.InnerException.Should().BeSameAs(requestException);
    }

    [TestMethod]
    public async Task UpdateOrganisationAsync_WhenUnexpectedExceptionOccurs_LogsAndRethrowsIt()
    {
        var expectedException = new InvalidOperationException("Unexpected failure");
        var logger = new RecordingLogger<OrganisationService>();
        var service = CreateService((_, _) => throw expectedException, logger);

        var action = () => service.UpdateOrganisationAsync(CreateDetails(), true, string.Empty);

        var exception = await action.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Should().BeSameAs(expectedException);
        logger.Entries.Should().ContainSingle(entry =>
            entry.Level == LogLevel.Error &&
            entry.Exception == expectedException);
    }

    [TestMethod]
    public async Task UpdateOrganisationAsync_WhenResponseIsInvalid_ThrowsOrganisationServiceException()
    {
        var service = CreateService((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not-json", Encoding.UTF8, "application/json")
        }));

        var action = () => service.UpdateOrganisationAsync(CreateDetails(), true, string.Empty);

        var exception = await action.Should().ThrowAsync<OrganisationServiceException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.OK);
        exception.Which.Message.Should().Be("The organisation service returned an invalid response.");
    }

    [TestMethod]
    public async Task UpdateOrganisationAsync_WhenCallerCancels_PreservesCancellation()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        var logger = new RecordingLogger<OrganisationService>();
        var service = CreateService(
            (_, cancellationToken) => throw new OperationCanceledException(cancellationToken),
            logger);

        var action = () => service.UpdateOrganisationAsync(
            CreateDetails(),
            true,
            string.Empty,
            cancellationTokenSource.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
        logger.Entries.Should().ContainSingle(entry => entry.Level == LogLevel.Warning);
    }

    private static OrganisationService CreateService(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync,
        ILogger<OrganisationService>? logger = null)
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
            logger ?? NullLogger<OrganisationService>.Instance);
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

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
    }

    private sealed record LogEntry(LogLevel Level, string Message, Exception? Exception);
}
