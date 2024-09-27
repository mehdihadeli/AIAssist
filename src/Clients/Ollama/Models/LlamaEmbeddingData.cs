using System.Text.Json.Serialization;

namespace Clients.Ollama.Models;

public class LlamaEmbeddingData
{
    [JsonPropertyName("embedding")]
    public IList<double> Embedding { get; set; } = new List<double>();
}
