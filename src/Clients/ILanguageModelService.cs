namespace Clients;

public interface ILanguageModelService
{
    Task<string> GetCompletionAsync(string userQuery, string codeContext);
    Task<string> GetEmbeddingAsync(string input);
}
