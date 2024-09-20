using Newtonsoft.Json;

namespace Clients.Olama.Models;

public class LlamaEmbeddingResponse
{
    [JsonProperty("data")]
    public List<LlamaEmbeddingData> Data { get; set; } = new();

    // Add other properties if needed
}
