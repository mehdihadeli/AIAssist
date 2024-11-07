using System.Text.Json.Serialization;

namespace Clients.Models.OpenAI.Completion;

public class OpenAIChoice
{
    public int Index { get; set; }
    public OpenAIMessage Message { get; set; } = new();
    public OpenAIMessage Delta { get; set; } = default!;

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = string.Empty;
}
