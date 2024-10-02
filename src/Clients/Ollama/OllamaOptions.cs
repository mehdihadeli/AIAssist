namespace Clients.Ollama;

public class OllamaOptions
{
    public required string BaseAddress { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "llama3.1";
    public string EmbeddingsModel { get; set; } = "llama3.1";
    public int MaxTokenSize { get; set; } = 8000;
}
