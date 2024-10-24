using Clients.Chat.Models;

namespace AIAssistant.Contracts;

public interface ILLMClientManager
{
    public string ChatModel { get; }
    public string EmbeddingModel { get; }
    public double EmbeddingThreshold { get; }
    IAsyncEnumerable<string?> GetCompletionStreamAsync(
        ChatSession chatSession,
        string userQuery,
        string systemContext,
        CancellationToken cancellationToken = default
    );
    Task<IList<double>> GetEmbeddingAsync(string input, CancellationToken cancellationToken = default);
}
