namespace Clients.Options;

public class LLMOptions
{
    public string BaseAddress { get; set; } = Constants.Ollama.BaseAddress;
    public string ApiKey { get; set; } = default!;
    public string ChatModel { get; set; } = Constants.Ollama.ChatModels.Deepseek_Coder_V2;
    public string EmbeddingsModel { get; set; } = Constants.Ollama.EmbeddingsModels.Nomic_EmbedText;
    public int MaxTokenSize { get; set; } = 12000;
    public double Temperature { get; set; } = 0.2;
}
