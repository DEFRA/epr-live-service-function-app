using System.Globalization;
using System.Net;
using Dapper;
using EPR.LiveService.FunctionApp.Formatting;
using EPR.LiveService.FunctionApp.Queries;
using EPR.LiveService.FunctionApp.Sql;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EPR.LiveService.FunctionApp.Functions;

public class RunQueryFunction
{
    private readonly IQueryRegistry _registry;
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly IServiceProvider _serviceProvider;

    public RunQueryFunction(
        IQueryRegistry registry,
        ISqlConnectionFactory connectionFactory,
        IServiceProvider serviceProvider)
    {
        _registry = registry;
        _connectionFactory = connectionFactory;
        _serviceProvider = serviceProvider;
    }

    [Function("RunQuery")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "query/{queryId}/results")] HttpRequestData req,
        string queryId)
    {
        QueryDefinition definition;
        try
        {
            definition = _registry.Get(queryId);
        }
        catch (KeyNotFoundException ex)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync(ex.Message);
            return notFound;
        }

        DynamicParameters parameters;
        try
        {
            parameters = BuildParameters(definition, req.Query);
        }
        catch (ArgumentException ex)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync(ex.Message);
            return badRequest;
        }

        var outputKey = req.Query.Get("output");
        if (string.IsNullOrWhiteSpace(outputKey))
        {
            outputKey = QueryOutputFormat.AsciiTable.Key();
        }

        if (!QueryOutputFormatDisplay.TryParseKey(outputKey, out var output))
        {
            var badOutput = req.CreateResponse(HttpStatusCode.BadRequest);
            await badOutput.WriteStringAsync($"Unknown option: {outputKey}");
            return badOutput;
        }

        using var connection = await _connectionFactory.CreateConnectionAsync(definition.Target);
        var sql = await _registry.LoadScriptAsync(queryId);

        var records = (await connection.QueryAsync(sql, parameters)).ToList();

        if (records.Count == 0)
        {
            var noContent = req.CreateResponse(HttpStatusCode.OK);
            noContent.Headers.Add("Content-Type", "text/html; charset=utf-8");
            await noContent.WriteStringAsync(AsciiTableFormatter.WrapAsFragment("(no rows)"));
            return noContent;
        }

        // Resolved eagerly for every QueryOutputFormat at startup in Program.cs,
        // so a missing formatter here would already have failed the app before
        // this request could ever arrive.
        var formatter = _serviceProvider.GetRequiredKeyedService<IQueryResultFormatter>(output);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await formatter.WriteAsync(response, queryId, records);
        return response;
    }

    private static DynamicParameters BuildParameters(QueryDefinition definition, System.Collections.Specialized.NameValueCollection query)
    {
        var parameters = new DynamicParameters();

        foreach (var paramDef in definition.Parameters)
        {
            var raw = query.Get(paramDef.Name);

            if (string.IsNullOrEmpty(raw))
            {
                if (paramDef.Required)
                {
                    throw new ArgumentException($"Missing required parameter '{paramDef.Name}'");
                }

                parameters.Add(paramDef.Name, null);
                continue;
            }

            object typedValue = paramDef.Type switch
            {
                "number" => decimal.Parse(raw),
                "date" => DateTime.Parse(raw, CultureInfo.InvariantCulture),
                _ => raw
            };

            parameters.Add(paramDef.Name, typedValue);
        }

        return parameters;
    }
}