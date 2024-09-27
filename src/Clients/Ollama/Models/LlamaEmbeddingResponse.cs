using System.Text.Json.Serialization;

namespace Clients.Ollama.Models;

public class LlamaEmbeddingResponse
{
    [JsonPropertyName("data")]
    public IList<LlamaEmbeddingData> Data { get; set; } = new List<LlamaEmbeddingData>();
}
