using System.Text.Json.Serialization;
using Clients.Converters;

namespace Clients.Models.Anthropic;

// ref: https://docs.anthropic.com/en/api/messages-examples
// ref: https://docs.anthropic.com/en/api/messages-examples
// https://docs.anthropic.com/en/api/messages-streaming#delta-types


public class AnthropicChatResponse
{
    public string Model { get; set; } = default!;
    public string Id { get; set; } = default!;
    public string Type { get; set; } = default!;

    [JsonConverter(typeof(RoleTypeConverter))]
    public RoleType Role { get; set; }
    public IList<MessageContent> Content { get; set; } = default!;

    [JsonPropertyName("delta")]
    public MessageContent? Delta { get; set; }

    [JsonPropertyName("error")]
    public AnthropicError? Error { get; set; }
    public AnthropicUsage? Usage { get; set; }

    [JsonPropertyName("stop_reason")]
    public string StopReason { get; set; } = default!;

    [JsonPropertyName("stop_sequence")]
    public string StopSequence { get; set; } = default!;
}

public class MessageContent
{
    public string Type { get; set; } = default!;
    public string Text { get; set; } = default!;
    public string? StopReason { get; set; }
    public string? StopSequence { get; set; }
}
