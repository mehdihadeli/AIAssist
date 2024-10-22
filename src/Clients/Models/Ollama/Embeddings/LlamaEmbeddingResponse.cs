using System.Text.Json.Serialization;

namespace Clients.Models.Ollama.Embeddings;

public class LlamaEmbeddingResponse
{
    [JsonPropertyName("data")]
    public IList<LlamaEmbeddingData> Data { get; set; } = new List<LlamaEmbeddingData>();
}
