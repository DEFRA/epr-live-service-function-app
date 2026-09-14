using System.Net;
using EPR.LiveService.FunctionApp.Functions;
using EPR.LiveService.FunctionApp.Queries;
using EPR.LiveService.FunctionApp.UnitTests.TestSupport.Fakes;
using EPR.LiveService.FunctionApp.UnitTests.TestSupport.Http;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Functions;

[TestClass]
public class RunQueryFunctionTests
{
    private static RunQueryFunction CreateFunction(FakeQueryRegistry registry) =>
        new(
            registry,
            new UnreachableSqlConnectionFactory(),
            new ServiceCollection().BuildServiceProvider(),
            new QueryResultLimit(new ConfigurationBuilder().Build()));

    [TestMethod]
    public async Task Run_WithUnknownQueryId_ShouldReturnNotFound()
    {
        var function = CreateFunction(new FakeQueryRegistry());
        var request = TestHttpRequestData.Get("/api/query/does-not-exist/results");

        var response = await function.Run(request, "does-not-exist");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task Run_WithMissingRequiredParameter_ShouldReturnBadRequest()
    {
        var registry = new FakeQueryRegistry();
        registry.Add(new QueryDefinition
        {
            Id = "user_lookup",
            Target = "accounts",
            Parameters = [new QueryParameterDefinition { Name = "Email", Required = true }]
        });
        var function = CreateFunction(registry);
        var request = TestHttpRequestData.Get("/api/query/user_lookup/results");

        var response = await function.Run(request, "user_lookup");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.ReadBodyAsString().Should().Contain("Email");
    }

    [TestMethod]
    public async Task Run_WithInvalidParameterValue_ShouldReturnBadRequest()
    {
        // Doubles as an end-to-end regression test for the FormatException →
        // ArgumentException fix in QueryParameterBinder — exercised through
        // the actual Function this time, not just the binder in isolation.
        var registry = new FakeQueryRegistry();
        registry.Add(new QueryDefinition
        {
            Id = "user_lookup",
            Target = "accounts",
            Parameters = [new QueryParameterDefinition { Name = "Amount", Type = "number" }]
        });
        var function = CreateFunction(registry);
        var request = TestHttpRequestData.Get(
            "/api/query/user_lookup/results",
            new Dictionary<string, string> { ["Amount"] = "not-a-number" });

        var response = await function.Run(request, "user_lookup");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.ReadBodyAsString().Should().Contain("Amount");
    }

    [TestMethod]
    public async Task Run_WithUnrecognisedOutputKey_ShouldReturnBadRequest()
    {
        var registry = new FakeQueryRegistry();
        registry.Add(new QueryDefinition { Id = "user_lookup", Target = "accounts" });
        var function = CreateFunction(registry);
        var request = TestHttpRequestData.Get(
            "/api/query/user_lookup/results",
            new Dictionary<string, string> { ["output"] = "pdf" });

        var response = await function.Run(request, "user_lookup");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.ReadBodyAsString().Should().Contain("pdf");
    }

    [TestMethod]
    public async Task Run_WithOutputFormatNotSupportedByTheQuery_ShouldReturnBadRequest()
    {
        var registry = new FakeQueryRegistry();
        registry.Add(new QueryDefinition
        {
            Id = "user_lookup",
            Target = "accounts",
            Outputs = [QueryOutputFormat.Csv] // AsciiTable (the default when no ?output= is given) deliberately excluded
        });
        var function = CreateFunction(registry);
        var request = TestHttpRequestData.Get("/api/query/user_lookup/results");

        var response = await function.Run(request, "user_lookup");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.ReadBodyAsString().Should().Contain("not supported");
    }
}
