using System.Text.Json.Serialization;

namespace Clients.Models.OpenAI;

public class OpenAIUsage
{
    /// <summary>
    /// Input tokens count
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    /// <summary>
    /// Output tokens count
    /// </summary>
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}
