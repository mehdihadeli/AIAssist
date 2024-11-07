using System.Net;

namespace Clients.Models.OpenAI;

public class OpenAIException(OpenAIError? error, HttpStatusCode statusCode)
    : HttpRequestException(
        !string.IsNullOrWhiteSpace(error?.Message) ? error.Message : error?.Code ?? statusCode.ToString(),
        null,
        statusCode
    )
{
    public OpenAIError Error { get; } =
        error ?? new OpenAIError { Message = statusCode.ToString(), Code = ((int)statusCode).ToString() };

    public HttpStatusCode HttpStatusCode { get; } = statusCode;
}
