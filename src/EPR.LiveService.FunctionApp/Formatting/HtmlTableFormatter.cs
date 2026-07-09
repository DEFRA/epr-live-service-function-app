using System.Net;
using System.Text;

namespace EPR.LiveService.FunctionApp.Formatting;

public static class HtmlTableFormatter
{
    public static string ToHtmlTable(IEnumerable<dynamic> rows)
    {
        var list = rows.Cast<IDictionary<string, object>>().ToList();
        if (list.Count == 0) return "<p>(no rows)</p>";

        var columns = list[0].Keys.ToList();

        var sb = new StringBuilder();
        sb.AppendLine("""<div id="query-results-list" class="query-results-list">""");
        sb.AppendLine("""<div class="table-scroll">""");
        sb.AppendLine("<table>");
        sb.AppendLine("<thead><tr>");
        foreach (var column in columns)
        {
            sb.AppendLine($"""<th class="sort" data-sort="{WebUtility.HtmlEncode(column)}">{WebUtility.HtmlEncode(column)}</th>""");
        }
        sb.AppendLine("</tr></thead>");

        sb.AppendLine("""<tbody class="list">""");
        foreach (var row in list)
        {
            sb.AppendLine("<tr>");
            foreach (var column in columns)
            {
                var value = row[column]?.ToString() ?? "NULL";
                sb.AppendLine($"""<td class="{WebUtility.HtmlEncode(column)}">{WebUtility.HtmlEncode(value)}</td>""");
            }
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");
        sb.AppendLine("</div>");
        sb.AppendLine("</div>");

        return sb.ToString();
    }
}