using Newtonsoft.Json;

namespace Clients.OpenAI.Models;

public class OpenAiEmbeddingResponse
{
    [JsonProperty("data")]
    public List<OpenAiEmbeddingData> Data { get; set; } = new();

    // Add other properties if needed
}
