using Microsoft.ML.Tokenizers;

namespace BuildingBlocks.LLM;

public static class TokenizerHelper
{
    public static int TokenCount(string model, string prompt)
    {
        // https://learn.microsoft.com/en-us/dotnet/machine-learning/whats-new/overview
        Tokenizer tokenizer = TiktokenTokenizer.CreateForModel(model);
        return tokenizer.CountTokens(prompt);
    }

    public static double[] CreateVectorTokens(string model, string prompt)
    {
        var tokens = TiktokenTokenizer
            .CreateForModel(model)
            .EncodeToTokens(text: prompt, out string _, considerNormalization: false);

        // Convert tokens to a double[] vector representation
        double[] tokenIds = tokens.Select(t => (double)t.Id).ToArray();

        return tokenIds;
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
}
