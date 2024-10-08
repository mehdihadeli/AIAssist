using System.Text.Json.Serialization;
using Clients.Converters;
using Clients.Models;

namespace Clients.Ollama.Models.Completion;

public class LlamaCompletionMessage
{
    [JsonConverter(typeof(RoleTypeConverter))]
    public RoleType Role { get; set; }
    public string Content { get; set; } = string.Empty;
}
