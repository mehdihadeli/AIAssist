namespace AIAssistant.Contracts.CodeAssist;

public interface ICodeAssist
{
    Task LoadCodeFiles(string contextWorkingDirectory, IList<string>? codeFiles);
    IAsyncEnumerable<string?> QueryChatCompletionAsync(string userQuery);
}
