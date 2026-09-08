using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace EPR.LiveService.FunctionApp.Queries;

public sealed class QueryResultLimit
{
    public int MaxRows { get; }

    public QueryResultLimit(IConfiguration configuration)
    {
        MaxRows = configuration.GetValue<int>("QueryResults:MaxRows", 1000);
        if (MaxRows < 1 || MaxRows == int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(configuration),
                "QueryResults:MaxRows must be between 1 and 2147483646.");
        }
    }

    // Fetch one extra row to detect overflow without returning a partial result.
    public string Apply(string sql) =>
        $"SET ROWCOUNT {MaxRows + 1};\n{sql}\n;SET ROWCOUNT 0;";

    public List<dynamic> Read(IDataReader reader)
    {
        var rows = new List<dynamic>();
        var parse = reader.GetRowParser<dynamic>();
        // Also bound materialisation if a script changes ROWCOUNT itself.
        while (rows.Count <= MaxRows && reader.Read())
        {
            rows.Add(parse(reader));
        }

        return rows;
    }
}
