using System.Text.Json.Serialization;
using Clients.Converters;

namespace Clients.Models.Ollama.Completion;

public class LlamaCompletionMessage
{
    [JsonConverter(typeof(RoleTypeConverter))]
    public RoleType Role { get; set; }

    public string Content { get; set; } = default!;
}
