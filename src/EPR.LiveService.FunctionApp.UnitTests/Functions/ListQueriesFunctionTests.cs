using System.Net;
using EPR.LiveService.FunctionApp.Functions;
using EPR.LiveService.FunctionApp.Queries;
using EPR.LiveService.FunctionApp.UnitTests.TestSupport.Fakes;
using EPR.LiveService.FunctionApp.UnitTests.TestSupport.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Functions;

[TestClass]
public class ListQueriesFunctionTests
{
    [TestMethod]
    public async Task Run_ShouldRenderAllRegisteredQueries()
    {
        var registry = new FakeQueryRegistry();
        registry.Add(new QueryDefinition
        {
            Id = "organisation_details",
            DisplayName = "Organisation details",
            Description = "Find an organisation"
        });
        var function = new ListQueriesFunction(registry);
        var request = TestHttpRequestData.Get("/api/queries");

        var response = await function.Run(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ReadBodyAsString().Should().Contain("Organisation details");
    }
}
