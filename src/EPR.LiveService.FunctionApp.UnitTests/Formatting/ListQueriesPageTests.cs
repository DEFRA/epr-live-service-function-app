using EPR.LiveService.FunctionApp.Formatting;
using EPR.LiveService.FunctionApp.Queries;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Formatting;

[TestClass]
public class ListQueriesPageTests
{
    [TestMethod]
    public void Build_WithNullDefinitions_ShouldThrowArgumentNullException()
    {
        var act = () => ListQueriesPage.Build(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void Build_ShouldOrderDefinitionsByDisplayName()
    {
        var definitions = new[]
        {
            new QueryDefinition { Id = "b", DisplayName = "Zebra query", Description = "Zebra" },
            new QueryDefinition { Id = "a", DisplayName = "Alpha query", Description = "Alpha" }
        };

        var html = ListQueriesPage.Build(definitions);

        html.IndexOf("Alpha query", StringComparison.Ordinal)
            .Should().BeLessThan(html.IndexOf("Zebra query", StringComparison.Ordinal));
    }

    [TestMethod]
    public void Build_WithNoDefinitions_ShouldStillRenderTheStaticQueriesTab()
    {
        var html = ListQueriesPage.Build([]);

        html.Should().Contain("href=\"/api/resend-invite-email\"");
    }
}
