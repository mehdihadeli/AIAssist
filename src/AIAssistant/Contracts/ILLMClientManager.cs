using AIAssistant.Dtos;
using Clients.Models;

namespace AIAssistant.Contracts;

public interface ILLMClientManager
{
    public Model ChatModel { get; }
    public Model? EmbeddingModel { get; }
    public decimal EmbeddingThreshold { get; }
    IAsyncEnumerable<string?> GetCompletionStreamAsync(
        string userQuery,
        string? systemContext,
        CancellationToken cancellationToken = default
    );
    Task<GetEmbeddingResult> GetEmbeddingAsync(string input, CancellationToken cancellationToken = default);
}
