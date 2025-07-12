using Clients.Dtos;

namespace Clients.Contracts;

public interface IChatClient
{
    IAsyncEnumerable<ChatCompletionResponse?> GetCompletionStreamAsync(
        ChatCompletionRequest chatCompletionRequest,
        CancellationToken cancellationToken = default
    );
}
