using Clients.Models;

namespace Clients.Contracts;

public interface ICacheModels
{
    Model? GetModel(string? modelName);
}
