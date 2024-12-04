namespace AIAssist.Contracts.CodeAssist;

public interface ICodeAssist
{
    // AddOrUpdate folders, sub-folders, files in summary
    Task LoadInitCodeFiles(string contextWorkingDirectory, IList<string>? codeFiles);

    // AddOrUpdate files with full definition
    Task AddOrUpdateCodeFiles(IList<string>? codeFiles);
    Task<IEnumerable<string>> GetCodeTreeContents(IList<string>? codeFiles);
    IAsyncEnumerable<string?> QueryChatCompletionAsync(string userQuery);
}
