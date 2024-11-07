using System.Text.Json.Serialization;

namespace Clients.Models.Anthropic;

public class AnthropicUsage
{
    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; set; }

    [JsonPropertyName("output_tokens")]
    public int OutputTokens { get; set; }

    public int TotalTokens => InputTokens + OutputTokens;
}
