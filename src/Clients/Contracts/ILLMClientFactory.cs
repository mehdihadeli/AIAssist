using Clients.Models;

namespace Clients.Contracts;

public interface ILLMClientFactory
{
    IChatClient CreateChatClient(AIProvider aiProvider);
    IEmbeddingsClient CreateEmbeddingsClient(AIProvider aiProvider);
}
