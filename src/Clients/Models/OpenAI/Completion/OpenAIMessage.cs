using System.Text.Json.Serialization;
using Clients.Converters;

namespace Clients.Models.OpenAI.Completion;

public class OpenAIMessage
{
    [JsonConverter(typeof(RoleTypeConverter))]
    public RoleType Role { get; set; }
    public string? Content { get; set; } = string.Empty;
}
