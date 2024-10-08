using Microsoft.ML.Tokenizers;

namespace BuildingBlocks.LLM;

// ref: https://learn.microsoft.com/en-us/dotnet/machine-learning/whats-new/overview
// https://github.com/dotnet/machinelearning/blob/4d5317e8090e158dc7c3bc6c435926ccf1cbd8e2/src/Microsoft.ML.Tokenizers/Model/Tiktoken.cs#L683-L734
// https://github.com/dotnet/machinelearning/blob/main/docs/code/microsoft-ml-tokenizers-migration-guide.md
public static class TokenizerHelper
{
    public static int GPT4TokenCount(string prompt)
    {
        // https://learn.microsoft.com/en-us/dotnet/machine-learning/whats-new/overview
        Tokenizer tokenizer = TiktokenTokenizer.CreateForModel("gpt-4");

        return tokenizer.CountTokens(prompt);
    }

    public static double[] GPT4VectorTokens(string prompt)
    {
        // https://learn.microsoft.com/en-us/dotnet/machine-learning/whats-new/overview#additional-tokenizer-support
        Tokenizer tokenizer = TiktokenTokenizer.CreateForModel("gpt-4");

        IReadOnlyList<int> encodedIds = tokenizer.EncodeToIds(prompt);

        return encodedIds.Select(x => (double)x).ToArray();
    }

    public static async Task<int> PhiTokenCount(string prompt)
    {
        // https://learn.microsoft.com/en-us/dotnet/machine-learning/whats-new/overview#additional-tokenizer-support
        string modelUrl =
            "https://huggingface.co/microsoft/Phi-3.5-mini-instruct/resolve/main/tokenizer.model?download=true";
        await using Stream remoteStream = await DownloadFileAsStreamAsync(modelUrl);
        Tokenizer llamaTokenizer = LlamaTokenizer.Create(remoteStream);

        return llamaTokenizer.CountTokens(prompt);
    }

    public static async Task<double[]> PhiVectorTokens(string prompt)
    {
        // https://learn.microsoft.com/en-us/dotnet/machine-learning/whats-new/overview#additional-tokenizer-support
        string modelUrl =
            "https://huggingface.co/microsoft/Phi-3.5-mini-instruct/resolve/main/tokenizer.model?download=true";
        await using Stream remoteStream = await DownloadFileAsStreamAsync(modelUrl);
        Tokenizer llamaTokenizer = LlamaTokenizer.Create(remoteStream);

        IReadOnlyList<int> encodedIds = llamaTokenizer.EncodeToIds(prompt);

        return encodedIds.Select(x => (double)x).ToArray();
    }

    public static int TokenCount(string text)
    {
        // If the input is long, we estimate using a sample of the input
        string[] lines = text.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
        int numLines = lines.Length;
        int step = Math.Max(numLines / 100, 1); // Ensure step is at least 1
        var sampledLines = lines.Where((_, index) => index % step == 0).ToArray();

        string sampleText = string.Join(Environment.NewLine, sampledLines);
        int sampleTokens = Tokenize(sampleText).Count;

        // Estimate total tokens based on the ratio of sampled input length to original input length
        double estimatedTokens = (double)sampleTokens / sampleText.Length * text.Length;

        return (int)Math.Round(estimatedTokens);

        // This function represents tokenizing the input, which you can implement based on your needs
        List<string> Tokenize(string input)
        {
            // A simple tokenization method based on spaces, but you can replace this with a more complex tokenizer
            return input.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }

    private static async Task<Stream> DownloadFileAsStreamAsync(string url)
    {
        using HttpClient client = new HttpClient();

        try
        {
            // Get the file as a stream
            Stream stream = await client.GetStreamAsync(url);
            return stream;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Error downloading file: " + e.Message);
            throw;
        }
    }
}
