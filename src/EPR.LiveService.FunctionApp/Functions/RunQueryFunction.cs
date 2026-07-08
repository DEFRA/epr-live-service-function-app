using System.Globalization;
using System.Net;
using CsvHelper;
using Dapper;
using EPR.LiveService.FunctionApp.Formatting;
using EPR.LiveService.FunctionApp.Queries;
using EPR.LiveService.FunctionApp.Sql;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace EPR.LiveService.FunctionApp.Functions;

public class RunQueryFunction
{
    private readonly IQueryRegistry _registry;
    private readonly ISqlConnectionFactory _connectionFactory;

    public RunQueryFunction(IQueryRegistry registry, ISqlConnectionFactory connectionFactory)
    {
        _registry = registry;
        _connectionFactory = connectionFactory;
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

        var output = req.Query.Get("output");
        if (string.IsNullOrWhiteSpace(output))
        {
            output = "ascii_table";
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

        switch (output)
        {
            case "csv":
            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                await using var streamWriter = new StreamWriter(response.Body, leaveOpen: true);
                await using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

                await csvWriter.WriteRecordsAsync(records);
                await csvWriter.FlushAsync();
                await streamWriter.FlushAsync();

                return response;
            }
            case "ascii_table":
            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/html; charset=utf-8");

                var table = AsciiTableFormatter.ToAsciiTable(records);
                await response.WriteStringAsync(AsciiTableFormatter.WrapAsFragment(table));

                return response;
            }
            default:
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync($"Unknown option: {output}");
                return badRequest;
            }
        }
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
                "date" => DateTime.Parse(raw),
                _ => raw
            };

            parameters.Add(paramDef.Name, typedValue);
        }

        return parameters;
    }
}