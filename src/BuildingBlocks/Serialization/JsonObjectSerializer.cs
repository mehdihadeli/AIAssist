using System.Text.Json;

namespace BuildingBlocks.Serialization;

public class JsonObjectSerializer : IJsonSerializer
{
    public JsonSerializerOptions Options =>
        new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, Options);
    }

    public T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, Options);
    }
}
