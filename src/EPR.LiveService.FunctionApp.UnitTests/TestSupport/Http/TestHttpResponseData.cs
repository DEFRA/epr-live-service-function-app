using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace EPR.LiveService.FunctionApp.UnitTests.TestSupport.Http;

internal sealed class TestHttpResponseData : HttpResponseData
{
    private TestHttpResponseData(FunctionContext context) : base(context)
    {
    }

    public override HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    public override HttpHeadersCollection Headers { get; set; } = new();
    public override Stream Body { get; set; } = new MemoryStream();
    public override HttpCookies Cookies =>
        throw new NotSupportedException("None of the Functions under test set cookies.");

    public static TestHttpResponseData Create(FunctionContext context) => new(context);
}
