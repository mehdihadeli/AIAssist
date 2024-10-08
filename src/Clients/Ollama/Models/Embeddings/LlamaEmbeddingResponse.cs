using System.Text.Json.Serialization;

namespace Clients.Ollama.Models.Embeddings;

public class LlamaEmbeddingResponse
{
    [JsonPropertyName("data")]
    public IList<LlamaEmbeddingData> Data { get; set; } = new List<LlamaEmbeddingData>();
}
