using Clients.Contracts;
using Clients.Models;

namespace Clients;

public class LLMClientFactory(IDictionary<AIProvider, ILLMClient> clientStrategies) : ILLMClientFactory
{
    public ILLMClient CreateClient(AIProvider aiProvider)
    {
        return clientStrategies[aiProvider];
    }
}
