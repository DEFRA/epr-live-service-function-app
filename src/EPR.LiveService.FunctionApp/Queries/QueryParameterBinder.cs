using System.Collections.Specialized;
using System.Globalization;
using Dapper;

namespace EPR.LiveService.FunctionApp.Queries;

public static class QueryParameterBinder
{
    public static DynamicParameters Build(QueryDefinition definition, NameValueCollection query)
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

            object typedValue;
            try
            {
                typedValue = paramDef.Type switch
                {
                    "number" => decimal.Parse(raw, CultureInfo.InvariantCulture),
                    "date" => DateTime.Parse(raw, CultureInfo.InvariantCulture),
                    _ => raw
                };
            }
            catch (Exception ex) when (ex is FormatException or OverflowException)
            {
                throw new ArgumentException(
                    $"Parameter '{paramDef.Name}' has an invalid {paramDef.Type} value: '{raw}'", ex);
            }

            parameters.Add(paramDef.Name, typedValue);
        }

        return parameters;
    }
}
