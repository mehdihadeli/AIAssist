namespace Clients.Models.OpenAI.Completion;

public class OpenAiCompletionResponse
{
    public string Id { get; set; } = default!;
    public string Object { get; set; } = default!;
    public long Created { get; set; }
    public string Model { get; set; } = default!;
    public string SystemFingerprint { get; set; } = default!;
    public IList<OpenAICompletionChoice> Choices { get; set; } = default!;
    public OpenAICompletionUsage Usage { get; set; } = default!;
}
