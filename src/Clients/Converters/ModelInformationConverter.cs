using System.Text.Json;
using System.Text.Json.Serialization;
using Clients.Models;

namespace Clients.Converters;

public class ModelInformationConverter : JsonConverter<Dictionary<string, ModelInformation>>
{
    public override Dictionary<string, ModelInformation> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var models = new Dictionary<string, ModelInformation>();

        using var jsonDoc = JsonDocument.ParseValue(ref reader);

        foreach (var property in jsonDoc.RootElement.EnumerateObject())
        {
            if (property.Name != "$schema") // Ignore "$schema"
            {
                var modelInfo = JsonSerializer.Deserialize<ModelInformation>(property.Value.GetRawText(), options);
                models[property.Name] = modelInfo!;
            }
        }

        return models;
    }

    public override void Write(
        Utf8JsonWriter writer,
        Dictionary<string, ModelInformation> value,
        JsonSerializerOptions options
    )
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}
