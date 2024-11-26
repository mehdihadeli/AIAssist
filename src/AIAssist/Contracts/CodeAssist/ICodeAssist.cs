namespace AIAssist.Contracts.CodeAssist;

public interface ICodeAssist
{
    Task LoadInitCodeFiles(string contextWorkingDirectory, IList<string>? codeFiles);
    Task AddOrUpdateCodeFiles(IList<string>? codeFiles);
    Task<IEnumerable<string>> GetCodeTreeContents(IList<string>? codeFiles);
    IAsyncEnumerable<string?> QueryChatCompletionAsync(string userQuery);
}
