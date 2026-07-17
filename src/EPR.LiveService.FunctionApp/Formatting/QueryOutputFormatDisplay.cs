using System.Collections.Concurrent;
using System.Reflection;
using EPR.LiveService.FunctionApp.Queries;

namespace EPR.LiveService.FunctionApp.Formatting;

/// <summary>
/// Reads the QueryOutputFormatMetadataAttribute attached to each QueryOutputFormat
/// member. Throws the first time a member is used if it's missing the attribute —
/// there's no compiler check for this (attributes can't be enforced at build time
/// the way an exhaustive switch expression can), so this is the fail-fast backstop.
/// </summary>
public static class QueryOutputFormatDisplay
{
    private static readonly ConcurrentDictionary<QueryOutputFormat, QueryOutputFormatMetadataAttribute> Metadata = new();

    public static string Key(this QueryOutputFormat format) => GetMetadata(format).Key;

    public static string DisplayName(this QueryOutputFormat format) => GetMetadata(format).DisplayName;

    public static string Hint(this QueryOutputFormat format) => GetMetadata(format).Hint;

    /// <summary>
    /// Parses a key (from the "output" query-string parameter, or from a query's
    /// .json Outputs list) back into a QueryOutputFormat.
    /// </summary>
    public static bool TryParseKey(string? key, out QueryOutputFormat format)
    {
        foreach (var candidate in Enum.GetValues<QueryOutputFormat>())
        {
            if (string.Equals(candidate.Key(), key, StringComparison.OrdinalIgnoreCase))
            {
                format = candidate;
                return true;
            }
        }

        format = default;
        return false;
    }

    private static QueryOutputFormatMetadataAttribute GetMetadata(QueryOutputFormat format) =>
        Metadata.GetOrAdd(format, static value =>
        {
            var field = typeof(QueryOutputFormat).GetField(value.ToString())
                ?? throw new InvalidOperationException($"QueryOutputFormat.{value} has no backing field");

            return field.GetCustomAttribute<QueryOutputFormatMetadataAttribute>()
                ?? throw new InvalidOperationException(
                    $"QueryOutputFormat.{value} is missing a [QueryOutputFormatMetadata] attribute");
        });
}