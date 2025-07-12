using Clients.Dtos;

namespace Clients.Contracts;

public interface IEmbeddingsClient
{
    Task<EmbeddingsResponse?> GetEmbeddingAsync(
        IList<string> inputs,
        string? path,
        CancellationToken cancellationToken = default
    );
}
