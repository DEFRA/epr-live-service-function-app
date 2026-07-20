using System.Globalization;
using CsvHelper;
using Microsoft.Azure.Functions.Worker.Http;

namespace EPR.LiveService.FunctionApp.Formatting;

/// <summary>
/// Streams query results as CSV directly to the response body via CsvHelper,
/// rather than buffering the whole file in memory — the reason large-dataset
/// queries restrict themselves to this format via QueryDefinition.Outputs.
/// </summary>
public class CsvFormatter : IQueryResultFormatter
{
    public async Task WriteAsync(HttpResponseData response, string queryId, IEnumerable<dynamic> records)
    {
        response.Headers.Add("Content-Type", "text/csv; charset=utf-8");
        response.Headers.Add("Content-Disposition", $"attachment; filename=\"{queryId}.csv\"");

        await using var streamWriter = new StreamWriter(response.Body, leaveOpen: true);
        await using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

        await csvWriter.WriteRecordsAsync(records);
        await csvWriter.FlushAsync();
        await streamWriter.FlushAsync();
    }
}