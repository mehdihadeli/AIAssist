using Clients.Chat.Models;
using Clients.Models;

namespace AIAssistant.Contracts;

public interface ILLMClientManager
{
    public string ChatModel { get; }
    public string EmbeddingModel { get; }
    public double EmbeddingThreshold { get; }
    Task<string?> GetCompletionAsync(
        ChatSession chatSession,
        string userQuery,
        string context,
        CancellationToken cancellationToken = default
    );
    IAsyncEnumerable<string?> GetCompletionStreamAsync(
        ChatSession chatSession,
        string userQuery,
        string context,
        CancellationToken cancellationToken = default
    );
    Task<IList<double>> GetEmbeddingAsync(string input, CancellationToken cancellationToken = default);
}
