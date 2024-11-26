namespace AIAssist.Dtos;

public class GetBatchEmbeddingResult(IList<IList<double>> embeddings, int totalTokensCount, decimal totalCost)
{
    public IList<IList<double>> Embeddings { get; } = embeddings;
    public int TotalTokensCount { get; } = totalTokensCount;
    public decimal TotalCost { get; } = totalCost;
}
