using System.Text.Json.Serialization;

namespace Clients.Models.Ollama;

public class OllamaResponseBase
{
    public string Model { get; set; } = default!;
    public long TotalDuration { get; set; }
    public long LoadDuration { get; set; }

    [JsonPropertyName("error")]
    public string Error { get; set; } = default!;

    /// <summary>
    /// output_tokens count
    /// </summary>
    public int EvalCount { get; set; }

    /// <summary>
    /// input_tokens count
    /// </summary>
    public int PromptEvalCount { get; set; }
    public int TotalTokensCount => EvalCount + PromptEvalCount;
}
