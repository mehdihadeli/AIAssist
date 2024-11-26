using System.Text.Json.Serialization;

namespace Clients.Models.OpenAI.Embeddings;

public class OpenAIEmbeddingResponse : OpenAIBaseResponse
{
    [JsonPropertyName("data")]
    public IList<OpenAiEmbeddingData>? Data { get; set; } = new List<OpenAiEmbeddingData>();
}
