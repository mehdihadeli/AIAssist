namespace Clients;

public interface ILanguageModelService
{
    Task<string> GetCompletionAsync(string userQuery, string codeContext);
    Task<IList<double>> GetEmbeddingAsync(string input);
}
