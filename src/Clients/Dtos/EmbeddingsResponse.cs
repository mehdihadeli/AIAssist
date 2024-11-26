namespace Clients.Dtos;

public record EmbeddingsResponse(IList<IList<double>>? Embeddings, TokenUsageResponse? TokenUsage);
