using Newtonsoft.Json;

namespace Clients.OpenAI.Models;

public class OpenAiCompletionChoice
{
    [JsonProperty("text")]
    public string Text { get; set; } = string.Empty;

    // Add other properties if needed
}
