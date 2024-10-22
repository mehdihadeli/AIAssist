namespace Clients.Models.OpenAI.Completion;

public class OpenAICompletionStreamResponse
{
    public string Id { get; set; } = default!;
    public string Object { get; set; } = default!;
    public long Created { get; set; }
    public string Model { get; set; } = default!;
    public string SystemFingerprint { get; set; } = default!;
    public IList<OpenAIStreamCompletionChoice> Choices { get; set; } = default!;
    public OpenAICompletionUsage Usage { get; set; } = default!;
}
