using Clients.Dtos;
using Clients.Models;

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
        string input,
        string? path,
        CancellationToken cancellationToken = default
    );
}
