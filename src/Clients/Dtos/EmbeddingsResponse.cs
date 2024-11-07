namespace Clients.Dtos;

public record EmbeddingsResponse(IList<double>? Embeddings, TokenUsageResponse? TokenUsage);
