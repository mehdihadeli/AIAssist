namespace AIAssist.Dtos;

public record GetEmbeddingResult(
    IList<IList<double>> Embeddings, // Multiple embeddings for batch
    int TotalTokensCount,
    decimal TotalCost
);
