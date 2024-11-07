using AIAssistant.Models;

namespace AIAssistant.Dtos;

public record GetRelatedEmbeddingsResult(
    IEnumerable<CodeEmbedding> CodeEmbeddings,
    int TotalTokensCount,
    decimal TotalCost
);
