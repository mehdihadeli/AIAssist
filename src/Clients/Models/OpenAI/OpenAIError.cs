using System.Text.Json.Serialization;

namespace Clients.Models.OpenAI;

// https://platform.openai.com/docs/guides/error-codes

public class OpenAIError
{
    public string Message { get; set; } = default!;

    public string Type { get; set; } = default!;

    [JsonPropertyName("param")]
    public string? Parameter { get; set; }

    public string? Code { get; set; }

    [JsonPropertyName("status")]
    public int StatusCode { get; set; }

    public OpenAIInnerInnerError? InnerError { get; set; }
}

public class OpenAIInnerInnerError
{
    public string Code { get; set; } = default!;
}
