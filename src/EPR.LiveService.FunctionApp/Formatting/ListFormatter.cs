using System.Text.RegularExpressions;
using Microsoft.Azure.Functions.Worker.Http;

namespace EPR.LiveService.FunctionApp.Formatting;

/// <summary>
/// Renders a single query result as a label/value report, turning absolute HTTP
/// and HTTPS values into links.
/// </summary>
public partial class ListFormatter : IQueryResultFormatter
{
    private static readonly IReadOnlyDictionary<string, string> FriendlyLabels =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["InvitedUserEmail"] = "Invitee Email",
            ["OrgRef"] = "Organisation ID"
        };

    public static string ToHtmlList(IEnumerable<dynamic> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        var rows = records.Cast<IDictionary<string, object>>().Take(2).ToList();
        if (rows.Count != 1)
        {
            throw new ArgumentException(
                $"List output requires exactly one record, but received {rows.Count}.",
                nameof(records));
        }

        var fields = rows[0].Select(field =>
        {
            var value = field.Value?.ToString() ?? "NULL";
            var isLink = Uri.TryCreate(value, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

            return new
            {
                Label = ToDisplayLabel(field.Key),
                Value = value,
                IsLink = isLink,
                IsCopyable = field.Value is string
            };
        }).ToArray();

        return TemplateRenderer.Render("List.sbn", new { Fields = fields });
    }

    public async Task WriteAsync(HttpResponseData response, string queryId, IEnumerable<dynamic> records)
    {
        response.Headers.Add("Content-Type", "text/html; charset=utf-8");
        await response.WriteStringAsync(ToHtmlList(records));
    }

    private static string ToDisplayLabel(string name)
    {
        if (FriendlyLabels.TryGetValue(name, out var friendlyLabel))
        {
            return friendlyLabel;
        }

        var withAcronymBoundaries = AcronymBoundaryRegex().Replace(name, "$1 $2");
        return WordBoundaryRegex().Replace(withAcronymBoundaries, "$1 $2");
    }

    [GeneratedRegex("([A-Z]+)([A-Z][a-z])")]
    private static partial Regex AcronymBoundaryRegex();

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex WordBoundaryRegex();
}
