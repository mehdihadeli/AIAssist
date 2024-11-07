namespace Clients.Options;

public class LLMOptions
{
    public string BaseAddress { get; set; } = default!;
    public string ApiKey { get; set; } = default!;
    public string ChatModel { get; set; } = default!;
    public string EmbeddingsModel { get; set; } = default!;
}
