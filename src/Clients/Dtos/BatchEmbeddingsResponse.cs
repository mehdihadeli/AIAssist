namespace Clients.Dtos;

public class BatchEmbeddingsResponse(IList<IList<double>> embeddings, int totalTokensCount, decimal totalCost)
{
    public IList<IList<double>> Embeddings { get; } = embeddings;
    public int TotalTokensCount { get; } = totalTokensCount;
    public decimal TotalCost { get; } = totalCost;
}
