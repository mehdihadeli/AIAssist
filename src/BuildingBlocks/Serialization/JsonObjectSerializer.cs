using System.Text.Json;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Serialization;

public class JsonObjectSerializer(JsonSerializerOptions options) : IJsonSerializer
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

    public static JsonSerializerOptions NoCaseOptions =>
        new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = null,
            // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/customize-properties?pivots=dotnet-8-0#enums-as-strings
            // naming policy for `writing` enum `values`
            Converters = { new JsonStringEnumConverter(null) },
        };

    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, options);
    }

    public T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, options);
    }
}
