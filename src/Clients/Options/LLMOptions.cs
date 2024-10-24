namespace Clients.Options;

public class LLMOptions
{
    public string BaseAddress { get; set; } = ClientsConstants.Ollama.BaseAddress;
    public string ApiKey { get; set; } = default!;
    public string ChatModel { get; set; } = ClientsConstants.Ollama.ChatModels.Llama3_1;
    public string EmbeddingsModel { get; set; } = ClientsConstants.Ollama.EmbeddingsModels.Nomic_EmbedText;
    public int MaxTokenSize { get; set; } = 12000;
    public double Temperature { get; set; } = 0.2;
}
