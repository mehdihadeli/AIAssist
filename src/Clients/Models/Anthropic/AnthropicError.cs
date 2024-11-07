using System.Net;

namespace Clients.Models.Anthropic;

public class AnthropicError
{
    public string Message { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Code { get; set; } = default!;
    public int StatusCode { get; set; } = default!;
}
