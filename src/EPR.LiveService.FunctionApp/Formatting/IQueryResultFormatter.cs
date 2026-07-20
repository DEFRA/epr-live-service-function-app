using Microsoft.Azure.Functions.Worker.Http;

namespace EPR.LiveService.FunctionApp.Formatting;

/// <summary>
/// Writes query result rows to an HTTP response in one particular format.
/// Implemented by HtmlTableFormatter, AsciiTableFormatter, and CsvFormatter —
/// RunQueryFunction resolves the right one for the requested QueryOutputFormat
/// rather than switching on the format itself.
/// </summary>
public interface IQueryResultFormatter
{
    Task WriteAsync(HttpResponseData response, string queryId, IEnumerable<dynamic> records);
}