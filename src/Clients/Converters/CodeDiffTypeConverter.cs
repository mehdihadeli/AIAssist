using System.Text.Json;
using System.Text.Json.Serialization;
using Clients.Models;
using Humanizer;

namespace Clients.Converters;

public class CodeDiffTypeConverter : JsonConverter<CodeDiffType>
{
    public override CodeDiffType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string modelType = reader.GetString() ?? string.Empty;

        var snakeModelType = modelType.Underscore();
        var codeBlock = CodeDiffType.CodeBlockDiff.ToString().Underscore();
        var unifiedDiff = CodeDiffType.UnifiedDiff.ToString().Underscore();
        var mergeConflict = CodeDiffType.MergeConflictDiff.ToString().Underscore();

        // Convert snake_case string to CodeDiff enum
        return snakeModelType switch
        {
            var type when type == codeBlock => CodeDiffType.CodeBlockDiff,
            var type when type == unifiedDiff => CodeDiffType.UnifiedDiff,
            var type when type == mergeConflict => CodeDiffType.MergeConflictDiff,
            _ => throw new JsonException($"Unknown CodeDiffType: {modelType}"),
        };
    }

    public override void Write(Utf8JsonWriter writer, CodeDiffType value, JsonSerializerOptions options)
    {
        string codeBlock = CodeDiffType.CodeBlockDiff.ToString().Underscore();
        string unifiedDiff = CodeDiffType.UnifiedDiff.ToString().Underscore();
        string mergeConflict = CodeDiffType.MergeConflictDiff.ToString().Underscore();

        // Convert CodeDiffType enum back to snake_case string
        string modelTypeString = value switch
        {
            CodeDiffType.CodeBlockDiff => codeBlock,
            CodeDiffType.UnifiedDiff => unifiedDiff,
            CodeDiffType.MergeConflictDiff => mergeConflict,
            _ => throw new JsonException($"Unknown CodeDiffType value: {value}"),
        };

        writer.WriteStringValue(modelTypeString);
    }
}
