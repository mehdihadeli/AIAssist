namespace Clients;

public interface ILanguageModelService
{
    Task<string> GetCompletionAsync(string prompt, string context);
    Task<string> GetEmbeddingAsync(string text);
}
