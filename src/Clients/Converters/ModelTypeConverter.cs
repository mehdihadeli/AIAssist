using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Clients.Models;
using Humanizer;

namespace Clients.Converters;

public class ModelTypeConverter : JsonConverter<ModelType>
{
    public override ModelType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Read the string value from JSON and convert it to snake_case
        string modelType = reader.GetString() ?? string.Empty;
        var snakeCaseType = modelType.Underscore();

        // Define snake_case mappings for each enum value
        var chat = ModelType.Chat.ToString().Underscore();
        var embedding = ModelType.Embedding.ToString().Underscore();

        // Convert snake_case string to ModelType enum
        return snakeCaseType switch
        {
            var type when type == chat => ModelType.Chat,
            var type when type == embedding => ModelType.Embedding,
            _ => throw new JsonException($"Unknown model type: {modelType}"),
        };
    }

    public override void Write(Utf8JsonWriter writer, ModelType value, JsonSerializerOptions options)
    {
        // Define snake_case strings for each enum value
        string chat = ModelType.Chat.ToString().Underscore();
        string embedding = ModelType.Embedding.ToString().Underscore();

        // Convert ModelType enum to corresponding snake_case string
        string modelTypeString = value switch
        {
            ModelType.Chat => chat,
            ModelType.Embedding => embedding,
            _ => throw new JsonException($"Unknown model type: {value}"),
        };

        writer.WriteStringValue(modelTypeString);
    }
}
