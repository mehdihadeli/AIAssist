using Newtonsoft.Json;

namespace Clients.Olama.Models;

public class LlamaCompletionChoice
{
    [JsonProperty("text")]
    public string Text { get; set; } = string.Empty;

    // Add other properties if needed
}
