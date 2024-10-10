using Clients.Models;

namespace Clients.Options;

public class LLMOptions
{
    public string BaseAddress { get; set; } = "http://localhost:11434";
    public string ApiKey { get; set; } = default!;
    public string ChatModel { get; set; } = Constants.Ollama.ChatModels.Llama3_1;
    public string EmbeddingsModel { get; set; } = Constants.Ollama.EmbeddingsModels.Mxbai_Embed_Large;
    public int MaxTokenSize { get; set; } = 8000;
    public AIProvider ProviderType { get; set; } = AIProvider.Ollama;
}
