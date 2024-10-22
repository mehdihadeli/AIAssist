using System.Text.Json.Serialization;

namespace Clients.Models.Ollama.Embeddings;

public class LlamaEmbeddingData
{
    [JsonPropertyName("embedding")]
    public IList<double> Embedding { get; set; } = new List<double>();
}
