using Newtonsoft.Json;

namespace Clients.OpenAI.Models;

public class OpenAiCompletionResponse
{
    [JsonProperty("choices")]
    public List<OpenAiCompletionChoice> Choices { get; set; } = new();

    // Add other properties if needed
}
