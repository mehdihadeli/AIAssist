using Clients.Models;

namespace Clients.Contracts;

public interface ILLMClientFactory
{
    ILLMClient CreateClient(AIProvider aiProvider);
}
