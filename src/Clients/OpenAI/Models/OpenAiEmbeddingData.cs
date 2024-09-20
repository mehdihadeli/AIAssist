using Newtonsoft.Json;

namespace Clients.OpenAI.Models;

public class OpenAiEmbeddingData
{
    [JsonProperty("embedding")]
    public List<double> Embedding { get; set; } = new();

    // Add other properties if needed
}
