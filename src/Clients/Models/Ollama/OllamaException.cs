using System.Net;

namespace Clients.Models.Ollama;

public class OllamaException(string? error, HttpStatusCode statusCode)
    : HttpRequestException(!string.IsNullOrWhiteSpace(error) ? error : statusCode.ToString(), null, statusCode)
{
    public string? Error { get; } = error;
    public HttpStatusCode HttpStatusCode { get; } = statusCode;
}
