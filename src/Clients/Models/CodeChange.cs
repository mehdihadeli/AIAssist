using Newtonsoft.Json;

namespace Clients.Models;

public class CodeChange
{
    [JsonProperty("fileRelativePath")]
    public string FileRelativePath { get; set; }

    [JsonProperty("beforeChange")]
    public string BeforeChange { get; set; }

    [JsonProperty("afterChange")]
    public string AfterChange { get; set; }
}
