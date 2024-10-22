using Clients.Models;

namespace Clients.Contracts;

public interface ILLMClientFactory
{
    ILLMClientStratgey CreateClient(AIProvider aiProvider);
}
