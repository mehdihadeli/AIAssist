namespace BuildingBlocks.LLM.Tokenizers;

// https://learn.microsoft.com/en-us/dotnet/machine-learning/whats-new/overview
public class CodeGenTokenizer(string vocabPath, string mergePath) : ITokenizer
{
    public Task<double[]> GetVectorTokens(string prompt)
    {
        using Stream vocabStream = File.OpenRead(vocabPath);
        using Stream mergesStream = File.OpenRead(mergePath);

        var codeGenTokenizer = Microsoft.ML.Tokenizers.CodeGenTokenizer.Create(vocabStream, mergesStream);

        IReadOnlyList<int> encodedIds = codeGenTokenizer.EncodeToIds(prompt);

        return Task.FromResult(encodedIds.Select(x => (double)x).ToArray());
    }

    public Task<int> GetTokenCount(string prompt)
    {
        using Stream vocabStream = File.OpenRead(vocabPath);
        using Stream mergesStream = File.OpenRead(mergePath);

        var codeGenTokenizer = Microsoft.ML.Tokenizers.CodeGenTokenizer.Create(vocabStream, mergesStream);

        return Task.FromResult(codeGenTokenizer.CountTokens(prompt));
    }
}
