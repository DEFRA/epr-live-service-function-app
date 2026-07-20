using System.Text.Json;
using System.Text.Json.Serialization;
using EPR.LiveService.FunctionApp.Queries;

namespace EPR.LiveService.FunctionApp.Formatting;

/// <summary>
/// Converts QueryOutputFormat to/from its explicit Key (e.g. "ascii_table"), as
/// declared via QueryOutputFormatMetadataAttribute on the enum member — rather
/// than a generic JsonStringEnumConverter naming policy, so the JSON
/// representation always matches the key defined right next to the enum member.
/// </summary>
public sealed class QueryOutputFormatJsonConverter : JsonConverter<QueryOutputFormat>
{
    public override QueryOutputFormat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var key = reader.GetString();

        if (!QueryOutputFormatDisplay.TryParseKey(key, out var format))
        {
            throw new JsonException($"'{key}' is not a recognised query output format");
        }

        return format;
    }

    public override void Write(Utf8JsonWriter writer, QueryOutputFormat value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Key());
}