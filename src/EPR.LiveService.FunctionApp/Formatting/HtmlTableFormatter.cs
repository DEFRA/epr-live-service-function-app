using System.Net;
using System.Text;
using Microsoft.Azure.Functions.Worker.Http;

namespace EPR.LiveService.FunctionApp.Formatting;

public class HtmlTableFormatter : IQueryResultFormatter
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

    public async Task WriteAsync(HttpResponseData response, string queryId, IEnumerable<dynamic> records)
    {
        response.Headers.Add("Content-Type", "text/html; charset=utf-8");
        await response.WriteStringAsync(ToHtmlTable(records));
    }
}
