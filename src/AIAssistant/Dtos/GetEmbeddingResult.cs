namespace AIAssistant.Dtos;

public record GetEmbeddingResult(IList<double> Embeddings, int TotalTokensCount, decimal TotalCost);
