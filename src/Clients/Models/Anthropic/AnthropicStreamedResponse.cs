using System.Text.Json.Serialization;
using Clients.Converters;

namespace Clients.Models.Anthropic;

// ref: https://docs.anthropic.com/en/api/messages-examples
// https://docs.anthropic.com/en/api/messages-streaming#delta-types

public class AnthropicStreamedResponse
{
    public string Id { get; set; } = default!;
    public string Type { get; set; } = default!;

    [JsonConverter(typeof(RoleTypeConverter))]
    public RoleType Role { get; set; }
    public IList<AnthropicDeltaStream> Content { get; set; } = default!;
    public string Model { get; set; } = default!;
    public string StopReason { get; set; } = default!;
    public string StopSequence { get; set; } = default!;
    public TokenUsage Usage { get; set; } = default!;
}
