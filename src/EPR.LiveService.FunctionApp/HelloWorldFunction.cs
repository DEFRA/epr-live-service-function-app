using CsvHelper;
using EPR.LiveService.FunctionApp.Repositories.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Globalization;
using System.Net;
using System.Text;

namespace MyFunctionApp;

public class HelloWorldFunction
{
    private readonly IOrganisationRepository _organisationRepository;

    public HelloWorldFunction(IOrganisationRepository organisationRepository)
    {
        _organisationRepository = organisationRepository;
    }

    [Function("HelloWorld")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        var orgRef = req.Query.Get("OrgRef");
        var option = req.Query.Get("option");

        if (string.IsNullOrWhiteSpace(orgRef))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Must provide OrgRef");
            return badRequest;
        }

        if(string.IsNullOrWhiteSpace(option))
        {
            option = "ascii_table"; // default to ascii_table if no option is provided
        }

        var records = (await _organisationRepository
            .GetOrganisationByOrgRefAsync(orgRef))
            ?.ToList();

        if (records == null || records.Count == 0)
        {
            return req.CreateResponse(HttpStatusCode.NoContent);
        }

        switch (option)
        {
            case "csv":

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                var streamWriter = new StreamWriter(response.Body, leaveOpen: true);
                var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

                await csvWriter.WriteRecordsAsync(records);
                await csvWriter.FlushAsync();
                await streamWriter.FlushAsync();

                return response;
            case "ascii_table":

                var response2 = req.CreateResponse(HttpStatusCode.OK);
                response2.Headers.Add("Content-Type", "text/html; charset=utf-8");
                await response2.WriteStringAsync(WrapAsHtml(ToAsciiTable(records)));

                return response2;

            default:
                throw new Exception($"Unknown option: {option}");

        }

    }

    public static string ToAsciiTable(IEnumerable<dynamic> rows)
    {
        var list = rows.Cast<IDictionary<string, object>>().ToList();
        if (!list.Any()) return "(no rows)";

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

    private static string WrapAsHtml(string asciiTable)
    {
        string escaped = System.Net.WebUtility.HtmlEncode(asciiTable);

        return $$"""
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="utf-8">
            <style>
                body \{margin: 0; padding: 1rem; background: #1e1e1e; \}
                pre \{font-family: 'Consolas', 'Courier New', monospace;
                    font-size: 13px;
                    color: #d4d4d4;
                    white-space: pre;
                    overflow-x: auto;
                    margin: 0;
                \}
            </style>
        </head>
        <body>
            <pre>{{escaped}}</pre>
        </body>
        </html>
        """;
    }
}