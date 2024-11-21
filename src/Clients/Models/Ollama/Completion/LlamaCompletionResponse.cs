using System.Text.Json.Serialization;
using Clients.Converters;

namespace Clients.Models.Ollama.Completion;

public class LlamaCompletionResponse : OllamaResponseBase
{
    public DateTime CreatedAt { get; set; }
    public LlamaCompletionMessage Message { get; set; } = default!;
    public string DoneReason { get; set; } = default!;
    public bool Done { get; set; }
    public long PromptEvalDuration { get; set; }
    public long EvalDuration { get; set; }
}

public class LlamaCompletionMessage
{
    [JsonConverter(typeof(RoleTypeConverter))]
    public RoleType Role { get; set; }

    public string? Content { get; set; } = default!;
}
