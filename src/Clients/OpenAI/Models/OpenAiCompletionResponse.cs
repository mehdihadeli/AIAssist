using System.Text.Json.Serialization;

namespace Clients.OpenAI.Models;

public class OpenAiCompletionResponse
{
    [JsonPropertyName("choices")]
    public IList<OpenAiCompletionChoice> Choices { get; set; } = new List<OpenAiCompletionChoice>();
}
