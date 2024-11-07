using Microsoft.ML.Tokenizers;

namespace BuildingBlocks.LLM.Tokenizers;

// https://learn.microsoft.com/en-us/dotnet/machine-learning/whats-new/overview

public class GptTokenizer(string modelName = "gpt-4") : ITokenizer
{
    public Task<double[]> GetVectorTokens(string prompt)
    {
        // https://learn.microsoft.com/en-us/dotnet/machine-learning/whats-new/overview#additional-tokenizer-support
        Tokenizer tokenizer = TiktokenTokenizer.CreateForModel(modelName);

        IReadOnlyList<int> encodedIds = tokenizer.EncodeToIds(prompt);

        return Task.FromResult(encodedIds.Select(x => (double)x).ToArray());
    }

    public Task<int> GetTokenCount(string prompt)
    {
        // https://learn.microsoft.com/en-us/dotnet/machine-learning/whats-new/overview
        Tokenizer tokenizer = TiktokenTokenizer.CreateForModel(modelName);

        return Task.FromResult(tokenizer.CountTokens(prompt));
    }
}
