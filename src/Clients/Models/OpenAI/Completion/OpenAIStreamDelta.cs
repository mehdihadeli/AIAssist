using System.Text.Json.Serialization;
using Clients.Converters;

namespace Clients.Models.OpenAI.Completion;

public class OpenAIStreamDelta
{
    public string Content { get; set; } = default!;

    [JsonConverter(typeof(RoleTypeConverter))]
    public RoleType Role { get; set; }
}
