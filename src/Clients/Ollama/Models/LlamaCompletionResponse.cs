using System.Text.Json.Serialization;

namespace Clients.Ollama.Models;

public class LlamaCompletionResponse
{
    [JsonPropertyName("choices")]
    public IList<LlamaCompletionChoice> Choices { get; set; } = new List<LlamaCompletionChoice>();
}
