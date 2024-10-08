using Clients.Models;

namespace Clients.Contracts;

public interface ILLMClientStratgey
{
    Task<string?> GetCompletionAsync(IReadOnlyList<ChatItem> chatItems);
    Task<IList<double>> GetEmbeddingAsync(string input);
}
