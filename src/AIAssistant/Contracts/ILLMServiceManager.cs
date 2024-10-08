using Clients.Models;

namespace AIAssistant.Contracts;

public interface ILLMServiceManager
{
    Task<string?> GetCompletionAsync(ChatSession chatSession, string userQuery, string context);
    Task<IList<double>> GetEmbeddingAsync(string input);
}
