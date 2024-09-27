using System.Text.Json.Serialization;

namespace Clients.Models;

public class CodeChangeResponse
{
    [JsonPropertyName("codeChanges")]
    public List<CodeChange> CodeChanges { get; set; }
}
