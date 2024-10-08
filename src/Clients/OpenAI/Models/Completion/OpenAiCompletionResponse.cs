namespace Clients.OpenAI.Models.Completion;

public class OpenAiCompletionResponse
{
    public string Id { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public long Created { get; set; }
    public string Model { get; set; } = string.Empty;
    public string SystemFingerprint { get; set; } = string.Empty;
    public IList<OpenAICompletionChoice> Choices { get; set; } = new List<OpenAICompletionChoice>();
    public OpenAICompletionUsage Usage { get; set; } = new OpenAICompletionUsage();
}
