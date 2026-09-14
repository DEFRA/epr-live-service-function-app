using System.Collections.Specialized;
using System.Globalization;
using EPR.LiveService.FunctionApp.Queries;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Queries;

[TestClass]
public class QueryParameterBinderTests
{
    [TestMethod]
    public void Build_WithMissingRequiredParameter_ShouldThrowArgumentException()
    {
        var definition = new QueryDefinition
        {
            Parameters = [new QueryParameterDefinition { Name = "ReferenceNumber", Required = true }]
        };

        var act = () => QueryParameterBinder.Build(definition, new NameValueCollection());

        act.Should().Throw<ArgumentException>().WithMessage("*ReferenceNumber*");
    }

    [TestMethod]
    public void Build_WithMissingOptionalParameter_ShouldBindNullRatherThanThrow()
    {
        var definition = new QueryDefinition
        {
            Parameters = [new QueryParameterDefinition { Name = "OrganisationId", Required = false }]
        };

        var parameters = QueryParameterBinder.Build(definition, new NameValueCollection());

        parameters.ParameterNames.Should().Contain("OrganisationId");
        parameters.Get<string?>("OrganisationId").Should().BeNull();
    }

    [TestMethod]
    public void Build_WithNumberParameter_ShouldConvertToDecimal()
    {
        var definition = new QueryDefinition
        {
            Parameters = [new QueryParameterDefinition { Name = "Amount", Type = "number" }]
        };
        var query = new NameValueCollection { { "Amount", "42.5" } };

        var parameters = QueryParameterBinder.Build(definition, query);

        parameters.Get<decimal>("Amount").Should().Be(42.5m);
    }

    [TestMethod]
    public void Build_WithDateParameter_ShouldConvertToDateTime()
    {
        var definition = new QueryDefinition
        {
            Parameters = [new QueryParameterDefinition { Name = "SubmittedFrom", Type = "date" }]
        };
        var query = new NameValueCollection { { "SubmittedFrom", "2026-01-15" } };

        var parameters = QueryParameterBinder.Build(definition, query);

        parameters.Get<DateTime>("SubmittedFrom").Should().Be(new DateTime(2026, 1, 15));
    }

    [TestMethod]
    public void Build_WithTextParameter_ShouldPassRawStringThrough()
    {
        var definition = new QueryDefinition
        {
            Parameters = [new QueryParameterDefinition { Name = "OrgRef", Type = "text" }]
        };
        var query = new NameValueCollection { { "OrgRef", "101 234" } };

        var parameters = QueryParameterBinder.Build(definition, query);

        parameters.Get<string>("OrgRef").Should().Be("101 234");
    }

    [TestMethod]
    public void Build_WithInvalidNumberParameter_ShouldThrowArgumentExceptionNamingTheParameterAndValue()
    {
        var definition = new QueryDefinition
        {
            Parameters = [new QueryParameterDefinition { Name = "Amount", Type = "number" }]
        };
        var query = new NameValueCollection { { "Amount", "not-a-number" } };

        var act = () => QueryParameterBinder.Build(definition, query);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Amount*")
            .WithMessage("*not-a-number*");
    }

    [TestMethod]
    public void Build_WithInvalidDateParameter_ShouldThrowArgumentExceptionNamingTheParameterAndValue()
    {
        var definition = new QueryDefinition
        {
            Parameters = [new QueryParameterDefinition { Name = "SubmittedFrom", Type = "date" }]
        };
        var query = new NameValueCollection { { "SubmittedFrom", "not-a-date" } };

        var act = () => QueryParameterBinder.Build(definition, query);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*SubmittedFrom*")
            .WithMessage("*not-a-date*");
    }

    [TestMethod]
    public void Build_WithOverflowingNumberParameter_ShouldThrowArgumentExceptionRatherThanOverflowException()
    {
        var definition = new QueryDefinition
        {
            Parameters = [new QueryParameterDefinition { Name = "Amount", Type = "number" }]
        };
        // One past decimal.MaxValue — decimal.Parse throws OverflowException for this, not FormatException.
        var query = new NameValueCollection { { "Amount", "79228162514264337593543950336" } };

        var act = () => QueryParameterBinder.Build(definition, query);

        act.Should().Throw<ArgumentException>().WithMessage("*Amount*");
    }

    [TestMethod]
    public void Build_WithNumberParameter_ShouldParseUsingInvariantCultureRegardlessOfThreadCulture()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE"); // comma decimal separator

        try
        {
            var definition = new QueryDefinition
            {
                Parameters = [new QueryParameterDefinition { Name = "Amount", Type = "number" }]
            };
            var query = new NameValueCollection { { "Amount", "42.5" } };

            var parameters = QueryParameterBinder.Build(definition, query);

            parameters.Get<decimal>("Amount").Should().Be(42.5m);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
