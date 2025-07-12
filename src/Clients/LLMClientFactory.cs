using Clients.Contracts;
using Clients.Models;

namespace Clients;

public class LLMClientFactory(IDictionary<AIProvider, IChatClient> clientChatStrategies, IDictionary<AIProvider, IEmbeddingsClient> clientEmbeddingsStrategies) : ILLMClientFactory
{
    public IChatClient CreateChatClient(AIProvider aiProvider)
    {
        return clientChatStrategies[aiProvider];
    }

    public IEmbeddingsClient CreateEmbeddingsClient(AIProvider aiProvider)
    {
        return clientEmbeddingsStrategies[aiProvider];
    }
}
