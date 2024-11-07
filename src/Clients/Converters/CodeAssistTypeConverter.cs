using System.Text.Json;
using System.Text.Json.Serialization;
using Clients.Models;
using Humanizer;

namespace Clients.Converters;

public class CodeAssistTypeConverter : JsonConverter<CodeAssistType>
{
    public override CodeAssistType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string modelType = reader.GetString() ?? string.Empty;

        var snakeModelType = modelType.Underscore();
        var embedding = CodeAssistType.Embedding.ToString().Underscore();
        var summary = CodeAssistType.Summary.ToString().Underscore();

        // Convert snake_case string to CodeDiff enum
        return snakeModelType switch
        {
            var type when type == embedding => CodeAssistType.Embedding,
            var type when type == summary => CodeAssistType.Summary,
            _ => throw new JsonException($"Unknown CodeAssistType: {modelType}"),
        };
    }

    public override void Write(Utf8JsonWriter writer, CodeAssistType value, JsonSerializerOptions options)
    {
        string embedding = CodeAssistType.Embedding.ToString().Underscore();
        string summary = CodeAssistType.Summary.ToString().Underscore();

        // Convert CodeDiffType enum back to snake_case string
        string modelTypeString = value switch
        {
            CodeAssistType.Embedding => embedding,
            CodeAssistType.Summary => summary,
            _ => throw new JsonException($"Unknown CodeAssistType value: {value}"),
        };

        writer.WriteStringValue(modelTypeString);
    }
}
