using System.Net;

namespace Clients.Models.Anthropic;

public class AnthropicException(AnthropicError? error, HttpStatusCode statusCode)
    : HttpRequestException(
        !string.IsNullOrWhiteSpace(error?.Message) ? error.Message : error?.Code ?? statusCode.ToString(),
        null,
        statusCode
    )
{
    public AnthropicError Error { get; } =
        error ?? new AnthropicError { Message = statusCode.ToString(), Code = ((int)statusCode).ToString() };

    public HttpStatusCode HttpStatusCode { get; } = statusCode;
}
