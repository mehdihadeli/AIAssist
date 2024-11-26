using Microsoft.ML.Tokenizers;

namespace BuildingBlocks.LLM.Tokenizers;

// https://learn.microsoft.com/en-us/dotnet/machine-learning/whats-new/overview
// https://platform.openai.com/docs/models

public class GptTokenizer(string modelName = "GPT-4o") : ITokenizer
{
    // https://learn.microsoft.com/en-us/dotnet/machine-learning/whats-new/overview#additional-tokenizer-support
    private readonly Tokenizer _tokenizer = TiktokenTokenizer.CreateForModel(modelName);

    public Task<double[]> GetVectorTokens(string prompt)
    {
        IReadOnlyList<int> encodedIds = _tokenizer.EncodeToIds(prompt);

        return Task.FromResult(encodedIds.Select(x => (double)x).ToArray());
    }

    public Task<int> GetTokenCount(string prompt)
    {
        // https://learn.microsoft.com/en-us/dotnet/machine-learning/whats-new/overview
        return Task.FromResult(_tokenizer.CountTokens(prompt));
    }
}
