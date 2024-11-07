namespace BuildingBlocks.LLM;

public interface ITokenizer
{
    Task<double[]> GetVectorTokens(string prompt);
    Task<int> GetTokenCount(string prompt);
}
