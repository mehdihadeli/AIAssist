using Newtonsoft.Json;

namespace Clients.Models;

public class SuggestedCodeResponse
{
    [JsonProperty("codeChanges")]
    public List<CodeChange> CodeChanges { get; set; }

    [JsonProperty("explanation")]
    public string Explanation { get; set; }
}
