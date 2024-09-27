using System.Text.Json.Serialization;

namespace Clients.Ollama.Models;

public class LlamaCompletionChoice
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
