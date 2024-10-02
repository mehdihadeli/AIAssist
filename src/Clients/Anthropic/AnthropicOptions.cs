namespace Clients.Anthropic;

public class AnthropicOptions
{
    public required string BaseAddress { get; set; }
    public required string ApiKey { get; set; }
    public string Model { get; set; } = "claude-3-5-sonnet-20240620";
    public string EmbeddingsModel { get; set; } = "claude-3-5-sonnet-20240620";
    public int MaxTokenSize { get; set; } = 8000;
}
