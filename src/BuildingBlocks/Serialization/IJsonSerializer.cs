using System.Text.Json;

namespace BuildingBlocks.Serialization;

public interface IJsonSerializer
{
    string Serialize<T>(T value);
    T? Deserialize<T>(string json);
}
