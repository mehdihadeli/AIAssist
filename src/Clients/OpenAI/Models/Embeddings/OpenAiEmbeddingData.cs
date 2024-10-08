using System.Text.Json.Serialization;

namespace Clients.OpenAI.Models.Embeddings;

public class OpenAiEmbeddingData
{
    [JsonPropertyName("embedding")]
    public IList<double> Embedding { get; set; } = new List<double>();
}
