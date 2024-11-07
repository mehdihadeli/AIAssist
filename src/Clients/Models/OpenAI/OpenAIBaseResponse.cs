namespace Clients.Models.OpenAI;

public class OpenAIBaseResponse
{
    public string Model { get; set; } = default!;
    public string Object { get; set; } = default!;
    public OpenAIUsage? Usage { get; set; }
    public OpenAIError? Error { get; set; }
}
