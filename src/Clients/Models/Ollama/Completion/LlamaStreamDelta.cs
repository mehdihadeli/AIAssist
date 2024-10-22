using System.Text.Json.Serialization;
using Clients.Converters;

namespace Clients.Models.Ollama.Completion;

public class LlamaStreamDelta
{
    public string Content { get; set; } = default!;

    [JsonConverter(typeof(RoleTypeConverter))]
    public RoleType Role { get; set; }
}
