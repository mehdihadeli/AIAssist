using Newtonsoft.Json;

namespace Clients.Olama.Models;

public class LlamaEmbeddingData
{
    [JsonProperty("embedding")]
    public List<double> Embedding { get; set; } = new();

    // Add other properties if needed
}
