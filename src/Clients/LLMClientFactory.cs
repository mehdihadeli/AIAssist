using Clients.Contracts;
using Clients.Models;

namespace Clients;

public class LLMClientFactory(IDictionary<AIProvider, ILLMClientStratgey> clientStrategies) : ILLMClientFactory
{
    public ILLMClientStratgey CreateClient(AIProvider aiProvider)
    {
        return clientStrategies[aiProvider];
    }
}
