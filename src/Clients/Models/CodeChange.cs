using System.Text.Json.Serialization;

namespace Clients.Models;

public class CodeChange
{
    [JsonPropertyName("fileRelativePath")]
    public string FileRelativePath { get; set; }

    [JsonPropertyName("beforeChange")]
    public string BeforeChange { get; set; }

    [JsonPropertyName("afterChange")]
    public string AfterChange { get; set; }

    [JsonPropertyName("diffs")]
    public List<string> Diffs { get; set; }

    [JsonPropertyName("explanation")]
    public string Explanation { get; set; }
}
