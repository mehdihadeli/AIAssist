using AIAssist.Models;

namespace AIAssist.Dtos;

public record GetRelatedEmbeddingsResult(
    IEnumerable<CodeEmbedding> CodeEmbeddings,
    int TotalTokensCount,
    decimal TotalCost
);
