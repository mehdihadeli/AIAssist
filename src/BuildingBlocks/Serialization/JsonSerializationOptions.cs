using System.Text.Json;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Serialization;

public static class JsonSerializationOptions
{
    public static JsonSerializerOptions SnakeCaseOptions =>
        new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/customize-properties?pivots=dotnet-8-0#enums-as-strings
            // naming policy for writing enum values
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) },
        };

    public static JsonSerializerOptions CamelCaseOptions =>
        new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/customize-properties?pivots=dotnet-8-0#enums-as-strings
            // naming policy for writing enum values
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

    public static JsonSerializerOptions NoCaseOptions =>
        new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = null,
            // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/customize-properties?pivots=dotnet-8-0#enums-as-strings
            // naming policy for `writing` enum `values`
            Converters = { new JsonStringEnumConverter(null) },
        };
}
