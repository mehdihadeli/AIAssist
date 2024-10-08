using System.Text.Json;

namespace BuildingBlocks.Serialization;

public interface IJsonSerializer
{
    public JsonSerializerOptions Options { get; }
    string Serialize<T>(T value);
    T? Deserialize<T>(string json);
}
