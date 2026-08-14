using System.Net;
using System.Net.Http.Headers;
using Azure.Core;
using EPR.LiveService.FunctionApp.Configs;
using EPR.LiveService.FunctionApp.Handlers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Handlers;

[TestClass]
public class OrganisationServiceAuthorisationHandlerTests
{
    private const string OrganisationServiceClientId = "11111111-1111-1111-1111-111111111111";

    [TestMethod]
    public async Task SendAsync_WhenNoAuthorizationHeaderIsSet_FetchesAndAttachesTokenForConfiguredClientId()
    {
        var credential = new FakeTokenCredential("service-token");
        using var handler = CreateHandler(credential);
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://organisation-service.test/api/thing");

        using var response = await SendAsync(handler, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.Authorization.Should().NotBeNull();
        request.Headers.Authorization!.Scheme.Should().Be("Bearer");
        request.Headers.Authorization.Parameter.Should().Be("service-token");
        credential.CallCount.Should().Be(1);
        credential.RequestedScopes.Should().ContainSingle().Which.Should().Be(OrganisationServiceClientId);
    }

    [TestMethod]
    public async Task SendAsync_WhenAuthorizationHeaderIsAlreadySet_DoesNotFetchTokenAndPreservesExistingHeader()
    {
        var credential = new FakeTokenCredential("service-token");
        using var handler = CreateHandler(credential);
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://organisation-service.test/api/thing");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "caller-supplied-token");

        using var response = await SendAsync(handler, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.Authorization!.Parameter.Should().Be("caller-supplied-token");
        credential.CallCount.Should().Be(0);
    }

    private static OrganisationServiceAuthorisationHandler CreateHandler(TokenCredential credential) =>
        new(CreateApiConfig(), credential)
        {
            InnerHandler = new StubHttpMessageHandler(
                (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)))
        };

    private static Task<HttpResponseMessage> SendAsync(
        HttpMessageHandler handler,
        HttpRequestMessage request) =>
        new HttpMessageInvoker(handler).SendAsync(request, CancellationToken.None);

    private static ApiConfig CreateApiConfig()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{ApiConfig.SectionName}:{nameof(ApiConfig.OrganisationServiceBaseUrl)}"] =
                    "https://organisation-service.test/",
                [$"{ApiConfig.SectionName}:{nameof(ApiConfig.OrganisationServiceClientId)}"] =
                    OrganisationServiceClientId,
                [$"{ApiConfig.SectionName}:{nameof(ApiConfig.Timeout)}"] = "30"
            })
            .Build();

        return ApiConfig.FromConfiguration(configuration.GetSection(ApiConfig.SectionName));
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => sendAsync(request, cancellationToken);
    }

    private sealed class FakeTokenCredential(string token) : TokenCredential
    {
        public List<string> RequestedScopes { get; } = [];

        public int CallCount { get; private set; }

        public override AccessToken GetToken(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken)
        {
            CallCount++;
            RequestedScopes.AddRange(requestContext.Scopes);
            return new AccessToken(token, DateTimeOffset.UtcNow.AddHours(1));
        }

        public override ValueTask<AccessToken> GetTokenAsync(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken)
        {
            CallCount++;
            RequestedScopes.AddRange(requestContext.Scopes);
            return ValueTask.FromResult(new AccessToken(token, DateTimeOffset.UtcNow.AddHours(1)));
        }
    }
}
