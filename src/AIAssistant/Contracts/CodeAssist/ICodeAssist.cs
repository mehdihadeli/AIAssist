namespace AIAssistant.Contracts.CodeAssist;

public interface ICodeAssist
{
    Task LoadInitCodeFiles(string contextWorkingDirectory, IList<string>? codeFiles);
    Task AddOrUpdateCodeFilesToCache(string contextWorkingDirectory, IList<string>? codeFiles);
    Task<IEnumerable<string>> GetCodeFilesFromCache(string contextWorkingDirectory, IList<string>? codeFiles);
    IAsyncEnumerable<string?> QueryChatCompletionAsync(string userQuery);
}
