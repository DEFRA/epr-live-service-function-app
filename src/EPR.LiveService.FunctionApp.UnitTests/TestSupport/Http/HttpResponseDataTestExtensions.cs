using Microsoft.Azure.Functions.Worker.Http;

namespace EPR.LiveService.FunctionApp.UnitTests.TestSupport.Http;

internal static class HttpResponseDataTestExtensions
{
    public static string ReadBodyAsString(this HttpResponseData response)
    {
        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body, leaveOpen: true);
        return reader.ReadToEnd();
    }
}
