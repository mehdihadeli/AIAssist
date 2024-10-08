namespace Clients.OpenAI.Models.Completion;

public class OpenAICompletionChoice
{
    public int Index { get; set; }
    public OpenAICompletionMessage Message { get; set; } = new();
    public string FinishReason { get; set; } = string.Empty;
}
