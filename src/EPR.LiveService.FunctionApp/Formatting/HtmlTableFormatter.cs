namespace EPR.LiveService.FunctionApp.Formatting;

public static class HtmlTableFormatter
{
    public static string ToHtmlTable(IEnumerable<dynamic> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var list = rows.Cast<IDictionary<string, object>>().ToList();
        var columns = list.Count == 0 ? [] : list[0].Keys.ToList();

        var model = new
        {
            Columns = columns,
            Rows = list.Select(row => new
            {
                Cells = columns.Select(column => new
                {
                    Column = column,
                    Value = row[column]?.ToString() ?? "NULL"
                }).ToArray()
            }).ToArray()
        };

        return TemplateRenderer.Render("HtmlTable.sbn", model);
    }
}
