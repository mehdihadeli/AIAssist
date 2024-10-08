using System.Text.Json.Serialization;
using Clients.Converters;
using Clients.Models;

namespace Clients.OpenAI.Models.Completion;

public class OpenAICompletionMessage
{
    [JsonConverter(typeof(RoleTypeConverter))]
    public RoleType Role { get; set; }
    public string Content { get; set; } = string.Empty;
}
