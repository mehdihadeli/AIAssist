using Clients.Models;

namespace Clients.Options;

public class LLMOptions
{
    public string ChatModel { get; set; } = default!;
    public string? EmbeddingsModel { get; set; }
    public CodeAssistType? CodeAssistType { get; set; }
    public CodeDiffType? CodeDiffType { get; set; }
    public decimal? Temperature { get; set; }
    public decimal? Threshold { get; set; }
}
