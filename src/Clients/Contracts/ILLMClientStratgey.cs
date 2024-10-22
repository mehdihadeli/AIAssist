using Clients.Chat.Models;
using Clients.Models;

namespace Clients.Contracts;

public interface ILLMClientStratgey
{
    Task<string?> GetCompletionAsync(IReadOnlyList<ChatItem> chatItems, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string?> GetCompletionStreamAsync(
        IReadOnlyList<ChatItem> chatItems,
        CancellationToken cancellationToken = default
    );
    Task<IList<double>> GetEmbeddingAsync(string input, CancellationToken cancellationToken = default);
}
