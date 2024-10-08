namespace Clients.Ollama.Models.Completion;

public class LlamaCompletionResponse
{
    public string Id { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public long Created { get; set; }
    public string Model { get; set; } = string.Empty;
    public string SystemFingerprint { get; set; } = string.Empty;
    public IList<LlamaCompletionChoice> Choices { get; set; } = new List<LlamaCompletionChoice>();
    public LlamaCompletionUsage Usage { get; set; } = new LlamaCompletionUsage();
}
