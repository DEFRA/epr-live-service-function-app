using EPR.LiveService.FunctionApp.Features;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Features;

[TestClass]
public class FeatureRegistryTests
{
    [TestMethod]
    public void ResendEprPackaging_ShouldBeDefinedAsNavigationMetadata()
    {
        var definition = new FeatureRegistry().All().Should().ContainSingle().Subject;

        definition.Id.Should().Be("resend_epr_packaging");
        definition.DisplayName.Should()
            .Be("Re-send Extended Producer Responsibility for Packaging Email");
        definition.Description.Should()
            .Be("Send the EPR packaging invitation email again using GOV.UK Notify.");
        definition.Path.Should().Be("/api/resend-epr-packaging");
    }

    [TestMethod]
    public void EveryFeatureDefinition_ShouldContainRequiredNavigationMetadata()
    {
        foreach (var definition in new FeatureRegistry().All())
        {
            definition.Id.Should().NotBeNullOrWhiteSpace();
            definition.DisplayName.Should().NotBeNullOrWhiteSpace();
            definition.Description.Should().NotBeNullOrWhiteSpace();
            definition.Path.Should().StartWith("/api/");
        }
    }
}
