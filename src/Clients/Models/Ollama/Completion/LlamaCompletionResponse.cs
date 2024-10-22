namespace Clients.Models.Ollama.Completion;

public class LlamaCompletionResponse
{
    public string Id { get; set; } = default!;
    public string Object { get; set; } = default!;
    public long Created { get; set; }
    public string Model { get; set; } = default!;
    public string SystemFingerprint { get; set; } = default!;
    public IList<LlamaCompletionChoice> Choices { get; set; } = default!;
    public LlamaCompletionUsage Usage { get; set; } = default!;
}
