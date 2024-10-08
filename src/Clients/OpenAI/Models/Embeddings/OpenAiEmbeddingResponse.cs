using System.Text.Json.Serialization;

namespace Clients.OpenAI.Models.Embeddings;

public class OpenAiEmbeddingResponse
{
    [JsonPropertyName("data")]
    public IList<OpenAiEmbeddingData> Data { get; set; } = new List<OpenAiEmbeddingData>();

    // Add other properties if needed
}
