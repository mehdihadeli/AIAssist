using System.Text.Json.Serialization;

namespace Clients.Models.Ollama.Embeddings;

public class LlamaEmbeddingResponse : OllamaResponseBase
{
    [JsonPropertyName("embeddings")]
    public IList<IList<double>>? Embeddings { get; set; } = default!;
}
