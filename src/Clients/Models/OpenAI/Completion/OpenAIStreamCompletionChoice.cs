namespace Clients.Models.OpenAI.Completion;

public class OpenAIStreamCompletionChoice
{
    public OpenAIStreamDelta Delta { get; set; } = default!;
    public int Index { get; set; }
    public string FinishReason { get; set; } = default!;
}
