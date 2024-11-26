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
        string? systemPrompt,
        CancellationToken cancellationToken = default
    );
    Task<GetEmbeddingResult> GetEmbeddingAsync(
        IList<string> inputs,
        string? path,
        CancellationToken cancellationToken = default
    );
}
