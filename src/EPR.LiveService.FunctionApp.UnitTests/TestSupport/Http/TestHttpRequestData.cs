using System.Security.Claims;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace EPR.LiveService.FunctionApp.UnitTests.TestSupport.Http;

internal sealed class TestHttpRequestData : HttpRequestData
{
    private TestHttpRequestData(FunctionContext context, string method, Uri url, Stream body)
        : base(context)
    {
        Method = method;
        Url = url;
        Body = body;
    }

    public override Stream Body { get; }
    public override HttpHeadersCollection Headers { get; } = new();
    public override IReadOnlyCollection<IHttpCookie> Cookies { get; } = Array.Empty<IHttpCookie>();
    public override Uri Url { get; }
    public override IEnumerable<ClaimsIdentity> Identities { get; } = Array.Empty<ClaimsIdentity>();
    public override string Method { get; }

    public override HttpResponseData CreateResponse() =>
        TestHttpResponseData.Create(FunctionContext);

    /// <summary>
    /// GET with an optional query string — covers ListQueriesFunction,
    /// QueryFormFunction, RunQueryFunction, and UserDetailsChangeFunction.ShowForm.
    /// </summary>
    public static TestHttpRequestData Get(string path, IDictionary<string, string>? query = null) =>
        new(new TestFunctionContext(), "GET", BuildUrl(path, query), new MemoryStream());

    /// <summary>
    /// POST with a JSON body — covers ResendInviteEmailFunction.Send and
    /// UserDetailsChangeFunction.RunQuery, both call req.ReadFromJsonAsync&lt;T&gt;().
    /// </summary>
    public static TestHttpRequestData PostJson(string path, string json)
    {
        var request = new TestHttpRequestData(
            new TestFunctionContext(), "POST", BuildUrl(path, null),
            new MemoryStream(Encoding.UTF8.GetBytes(json)));
        request.Headers.Add("Content-Type", "application/json");
        return request;
    }

    /// <summary>
    /// POST with a form-urlencoded body — covers ResendInviteEmailFunction.ShowForm's
    /// prefill-from-POST path, which reads req.Body directly rather than req.Query.
    /// </summary>
    public static TestHttpRequestData PostForm(string path, string formUrlEncodedBody)
    {
        var request = new TestHttpRequestData(
            new TestFunctionContext(), "POST", BuildUrl(path, null),
            new MemoryStream(Encoding.UTF8.GetBytes(formUrlEncodedBody)));
        request.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
        return request;
    }

    private static Uri BuildUrl(string path, IDictionary<string, string>? query)
    {
        var builder = new UriBuilder("http", "localhost", 80, path);
        if (query is { Count: > 0 })
        {
            builder.Query = string.Join(
                "&",
                query.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
        }
        return builder.Uri;
    }
}
