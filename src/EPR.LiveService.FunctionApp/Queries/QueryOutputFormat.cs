namespace EPR.LiveService.FunctionApp.Queries;

/// <summary>
/// The output formats RunQueryFunction can produce. Query definitions declare
/// which of these they support via QueryDefinition.Outputs. Each member carries
/// its own key/display name/hint via QueryOutputFormatMetadataAttribute - see
/// QueryOutputFormatDisplay for how those are read.
/// </summary>
public enum QueryOutputFormat
{
    [QueryOutputFormatMetadata("html", "HTML Table", "sortable by column")]
    Html,

    [QueryOutputFormatMetadata("ascii_table", "ASCII Table", "good for copy-pasting, nicely formatted")]
    AsciiTable,

    [QueryOutputFormatMetadata("csv", "CSV", "best for large result sets")]
    Csv
}