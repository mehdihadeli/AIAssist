namespace Clients.Models.Anthropic;

public class AnthropicDeltaStream
{
    public string Type { get; set; } = default!;
    public string Text { get; set; } = default!;
}
