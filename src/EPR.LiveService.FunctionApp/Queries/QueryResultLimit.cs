using System.Data;
using Dapper;

namespace EPR.LiveService.FunctionApp.Queries;

public static class QueryResultLimit
{
    public const int MaxRows = 100;

    // Fetch one extra row to detect overflow without returning a partial result.
    public static string Apply(string sql) =>
        $"SET ROWCOUNT {MaxRows + 1};\n{sql}\n;SET ROWCOUNT 0;";

    public static List<dynamic> Read(IDataReader reader)
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
