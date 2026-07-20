namespace EPR.LiveService.FunctionApp.Queries;

/// <summary>
/// Attaches the outward-facing key, display name, and hint to a QueryOutputFormat
/// member. Keeping these explicit and right next to the enum member they belong
/// to means there's one place to look, and one place to update when adding a
/// format.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class QueryOutputFormatMetadataAttribute(string key, string displayName, string hint) : Attribute
{

  /// <summary>The value used in "output" query strings and .json Outputs lists, e.g. "ascii_table".</summary>
  public string Key { get; } = key;

  /// <summary>The label shown next to this option's radio button, e.g. "ASCII Table".</summary>
  public string DisplayName { get; } = displayName;

  /// <summary>The short explanatory hint shown alongside the label.</summary>
  public string Hint { get; } = hint;
}