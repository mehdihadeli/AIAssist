namespace Clients.OpenAI;

public class OpenAIOptions
{
    public required string BaseAddress { get; set; }
    public required string ApiKey { get; set; }
    public string Model { get; set; } = "gpt-4o";
    public string EmbeddingsModel { get; set; } = "gpt-4o";
    public int MaxTokenSize { get; set; } = 8000;
}
