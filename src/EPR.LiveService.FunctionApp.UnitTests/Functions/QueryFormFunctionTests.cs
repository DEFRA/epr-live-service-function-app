using System.Net;
using EPR.LiveService.FunctionApp.Functions;
using EPR.LiveService.FunctionApp.Queries;
using EPR.LiveService.FunctionApp.UnitTests.TestSupport.Fakes;
using EPR.LiveService.FunctionApp.UnitTests.TestSupport.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Functions;

[TestClass]
public class QueryFormFunctionTests
{
    [TestMethod]
    public async Task Run_WithUnknownQueryId_ShouldReturnNotFound()
    {
        var function = new QueryFormFunction(new FakeQueryRegistry());
        var request = TestHttpRequestData.Get("/api/query/does-not-exist");

        var response = await function.Run(request, "does-not-exist");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.ReadBodyAsString().Should().Contain("does-not-exist");
    }

    [TestMethod]
    public async Task Run_WithKnownQueryId_ShouldRenderTheQueryForm()
    {
        var registry = new FakeQueryRegistry();
        registry.Add(new QueryDefinition
        {
            Id = "organisation_details",
            DisplayName = "Organisation details",
            Description = "Find an organisation",
            Parameters = []
        });
        var function = new QueryFormFunction(registry);
        var request = TestHttpRequestData.Get("/api/query/organisation_details");

        var response = await function.Run(request, "organisation_details");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ReadBodyAsString().Should().Contain("Organisation details");
    }
}
