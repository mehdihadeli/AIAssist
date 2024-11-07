namespace Clients.Models.OpenAI.Completion;

public class OpenAIChatResponse : OpenAIBaseResponse
{
    public string Id { get; set; } = default!;
    public long Created { get; set; }
    public string SystemFingerprint { get; set; } = default!;
    public IList<OpenAIChoice>? Choices { get; set; } = default!;
}
