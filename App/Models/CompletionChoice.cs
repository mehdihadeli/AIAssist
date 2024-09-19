using Newtonsoft.Json;

namespace AIRefactorAssistant.Models;

class CompletionChoice
{
    [JsonProperty("text")]
    public string Text { get; set; }
}
