namespace EPR.LiveService.FunctionApp.Features;

public interface IFeatureRegistry
{
    IEnumerable<FeatureDefinition> All();
}
