using EPR.LiveService.FunctionApp.Repositories.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;

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

        if(string.IsNullOrWhiteSpace(orgRef))
        {
          var badrequest = req.CreateResponse(HttpStatusCode.BadRequest);
          await badrequest.WriteStringAsync("Must provide OrgRef");
          return badrequest;
        }

        var organisationQueryResponse = await _organisationRepository.GetOrganisationByOrgRefAsync(orgRef);

        if(organisationQueryResponse.Count() ==0)
        {
            return req.CreateResponse(HttpStatusCode.NoContent);
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync(JsonSerializer.Serialize(organisationQueryResponse));

        return response;
    }
}