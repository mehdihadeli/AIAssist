using AIAssistant.Models;

namespace AIAssistant.Contracts.CodeAssist;

public interface ICodeAssistantManager
{
    Task LoadCodeFiles(string contextWorkingDirectory, IList<string>? codeFiles);
    IAsyncEnumerable<string?> QueryAsync(string userQuery);
    IList<FileChange> ParseResponseCodeBlocks(string response);
    void ApplyChangesToFiles(IList<FileChange> codeBlocks, string contextWorkingDirectory);
}
