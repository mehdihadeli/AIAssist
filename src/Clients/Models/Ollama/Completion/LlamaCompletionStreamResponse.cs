namespace Clients.Models.Ollama.Completion;

public class LlamaCompletionStreamResponse
{
    public string Id { get; set; } = default!;
    public string Object { get; set; } = default!;
    public long Created { get; set; }
    public string Model { get; set; } = default!;
    public string SystemFingerprint { get; set; } = default!;
    public IList<LlamaStreamCompletionChoice> Choices { get; set; } = default!;
    public LlamaCompletionUsage Usage { get; set; } = default!;
}
