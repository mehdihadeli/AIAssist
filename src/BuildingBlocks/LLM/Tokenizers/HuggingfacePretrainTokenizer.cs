namespace BuildingBlocks.LLM.Tokenizers;

// probably using ONNX models with ML.Net
//https://learn.microsoft.com/en-us/dotnet/machine-learning/tutorials/object-detection-onnx

public class HuggingfacePretrainTokenizer(string huggingfaceModelUrl) : ITokenizer
{
    public Task<double[]> GetVectorTokens(string prompt)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetTokenCount(string prompt)
    {
        throw new NotImplementedException();
    }
}
