using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace MyFunctionApp;

public class HelloWorldFunction
{
    [Function("HelloWorld")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        var orgRef = req.Query.Get("OrgRef");

        if(string.IsNullOrWhiteSpace(orgRef))
        {
          var badrequest = req.CreateResponse(HttpStatusCode.BadRequest);
          await badrequest.WriteStringAsync("Must provide OrgRef");
          return badrequest;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync(orgRef);

        return response;
    }
}