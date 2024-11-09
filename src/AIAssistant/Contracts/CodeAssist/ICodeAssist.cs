namespace AIAssistant.Contracts.CodeAssist;

public interface ICodeAssist
{
    Task LoadInitCodeFiles(string contextWorkingDirectory, IList<string>? codeFiles);
    Task AddOrUpdateCodeFilesToCache(IList<string>? codeFiles);
    Task<IEnumerable<string>> GetCodeTreeContentsFromCache(IList<string>? codeFiles);
    IAsyncEnumerable<string?> QueryChatCompletionAsync(string userQuery);
}
