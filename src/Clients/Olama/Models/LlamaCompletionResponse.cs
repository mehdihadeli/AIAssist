using Newtonsoft.Json;

namespace Clients.Olama.Models;

public class LlamaCompletionResponse
{
    [JsonProperty("choices")]
    public List<LlamaCompletionChoice> Choices { get; set; } = new();

    // Add other properties if needed
}
