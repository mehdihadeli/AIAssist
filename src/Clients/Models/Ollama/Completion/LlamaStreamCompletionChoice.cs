namespace Clients.Models.Ollama.Completion;

public class LlamaStreamCompletionChoice
{
    public LlamaStreamDelta Delta { get; set; } = default!;
    public int Index { get; set; }
    public string FinishReason { get; set; } = default!;
}
