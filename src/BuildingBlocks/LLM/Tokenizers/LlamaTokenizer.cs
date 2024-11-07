using Microsoft.ML.Tokenizers;

namespace BuildingBlocks.LLM.Tokenizers;

// https://learn.microsoft.com/en-us/dotnet/machine-learning/whats-new/overview

public class LlamaTokenizer(LlamaTokenizerType llamaTokenizerType) : ITokenizer
{
    private static readonly Dictionary<LlamaTokenizerType, byte[]> _tokenizerCache = new();

    public async Task<double[]> GetVectorTokens(string prompt)
    {
        var modelData = await GetOrDownloadModelDataAsync(llamaTokenizerType);
        using var memoryStream = new MemoryStream(modelData);
        Tokenizer llamaTokenizer = Microsoft.ML.Tokenizers.LlamaTokenizer.Create(memoryStream);

        IReadOnlyList<int> encodedIds = llamaTokenizer.EncodeToIds(prompt);
        return encodedIds.Select(x => (double)x).ToArray();
    }

    public async Task<int> GetTokenCount(string prompt)
    {
        var modelData = await GetOrDownloadModelDataAsync(llamaTokenizerType);
        using var memoryStream = new MemoryStream(modelData);
        Tokenizer llamaTokenizer = Microsoft.ML.Tokenizers.LlamaTokenizer.Create(memoryStream);

        return llamaTokenizer.CountTokens(prompt);
    }

    private async Task<byte[]> GetOrDownloadModelDataAsync(LlamaTokenizerType tokenizerType)
    {
        // Check if model data is already cached in memory
        if (_tokenizerCache.TryGetValue(tokenizerType, out var modelData))
        {
            return modelData;
        }

        // Download the model data and cache it in memory
        var url = GetUrl(tokenizerType);
        modelData = await DownloadFileAsByteArrayAsync(url);

        _tokenizerCache[tokenizerType] = modelData;
        return modelData;
    }

    private async Task<byte[]> DownloadFileAsByteArrayAsync(string url)
    {
        using HttpClient client = new HttpClient();
        try
        {
            return await client.GetByteArrayAsync(url);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Error downloading file: " + e.Message);
            throw;
        }
    }

    private string GetUrl(LlamaTokenizerType tokenizerType)
    {
        return tokenizerType switch
        {
            LlamaTokenizerType.Llama3_1_8b =>
                "https://huggingface.co/hf-internal-testing/llama-tokenizer/resolve/main/tokenizer.model",
            LlamaTokenizerType.Phi_3_5_Mini_Instruct =>
                "https://huggingface.co/microsoft/Phi-3.5-mini-instruct/resolve/main/tokenizer.model",
            LlamaTokenizerType.Phi_3_5_MoE_Instruct =>
                "https://huggingface.co/microsoft/Phi-3.5-MoE-instruct/resolve/main/tokenizer.model",
            _ => throw new Exception("Model not found."),
        };
    }
}

public enum LlamaTokenizerType
{
    Llama3_1_8b,
    Phi_3_5_MoE_Instruct,
    Phi_3_5_Mini_Instruct,
}
