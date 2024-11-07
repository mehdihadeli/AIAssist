using System.Text.Json;
using System.Text.Json.Serialization;
using Clients.Models;
using Humanizer;

namespace Clients.Converters;

public class AIProviderTypeConverter : JsonConverter<AIProvider>
{
    public override AIProvider Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Read the string value from JSON and convert it to snake_case
        string aiProviderValue = reader.GetString() ?? string.Empty;
        var snakeCaseValue = aiProviderValue.Underscore();

        // Define snake_case mappings for each enum value
        var openAI = AIProvider.Openai.ToString().Underscore();
        var ollama = AIProvider.Ollama.ToString().Underscore();
        var azure = AIProvider.Azure.ToString().Underscore();
        var anthropic = AIProvider.Anthropic.ToString().Underscore();

        // Convert snake_case string to AIProvider enum value
        return snakeCaseValue switch
        {
            var type when type == openAI => AIProvider.Openai,
            var type when type == ollama => AIProvider.Ollama,
            var type when type == azure => AIProvider.Azure,
            var type when type == anthropic => AIProvider.Anthropic,
            _ => throw new JsonException($"Unknown AIProvider type: {aiProviderValue}"),
        };
    }

    public override void Write(Utf8JsonWriter writer, AIProvider value, JsonSerializerOptions options)
    {
        // Define snake_case strings for each enum value
        string openAI = AIProvider.Openai.ToString().Underscore();
        string ollama = AIProvider.Ollama.ToString().Underscore();
        string azure = AIProvider.Azure.ToString().Underscore();
        string anthropic = AIProvider.Anthropic.ToString().Underscore();

        // Convert AIProvider enum to corresponding snake_case string
        string aiProviderString = value switch
        {
            AIProvider.Openai => openAI,
            AIProvider.Ollama => ollama,
            AIProvider.Azure => azure,
            AIProvider.Anthropic => anthropic,
            _ => throw new JsonException($"Unknown AIProvider type: {value}"),
        };

        writer.WriteStringValue(aiProviderString);
    }
}
