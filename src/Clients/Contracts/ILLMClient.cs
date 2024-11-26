using Clients.Dtos;

namespace Clients.Contracts;

public interface ILLMClient
{
    Task<ChatCompletionResponse?> GetCompletionAsync(
        ChatCompletionRequest chatCompletionRequest,
        CancellationToken cancellationToken = default
    );
    IAsyncEnumerable<ChatCompletionResponse?> GetCompletionStreamAsync(
        ChatCompletionRequest chatCompletionRequest,
        CancellationToken cancellationToken = default
    );
    Task<EmbeddingsResponse?> GetEmbeddingAsync(
        IList<string> inputs,
        string? path,
        CancellationToken cancellationToken = default
    );
}
