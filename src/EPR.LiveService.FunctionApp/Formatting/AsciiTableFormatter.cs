using System.Text;

namespace EPR.LiveService.FunctionApp.Formatting;

public static class AsciiTableFormatter
{
    public static string ToAsciiTable(IEnumerable<dynamic> rows)
    {
        var list = rows.Cast<IDictionary<string, object>>().ToList();
        if (list.Count == 0) return "(no rows)";

        var columns = list[0].Keys.ToList();
        var widths = columns.Select(c => Math.Max(c.Length,
            list.Max(r => (r[c]?.ToString() ?? "NULL").Length))).ToList();

        string Sep() => "+" + string.Join("+", widths.Select(w => new string('-', w + 2))) + "+";
        string Row(IEnumerable<string> cells) =>
            "| " + string.Join(" | ", cells.Select((c, i) => c.PadRight(widths[i]))) + " |";

        var sb = new StringBuilder();
        sb.AppendLine(Sep());
        sb.AppendLine(Row(columns));
        sb.AppendLine(Sep());
        foreach (var r in list)
            sb.AppendLine(Row(columns.Select(c => r[c]?.ToString() ?? "NULL")));
        sb.AppendLine(Sep());

        return sb.ToString();
    }

    public static string WrapAsFragment(string asciiTable) =>
        TemplateRenderer.Render("AsciiFragment.sbn", new { AsciiTable = asciiTable }).TrimEnd();
}
