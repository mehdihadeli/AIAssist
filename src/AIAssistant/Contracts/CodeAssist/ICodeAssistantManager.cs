using AIAssistant.Models;
using Clients.Chat.Models;

namespace AIAssistant.Contracts.CodeAssist;

public interface ICodeAssistantManager
{
    Task LoadCodeFiles(ChatSession chatSession, string? contextWorkingDirectory, IList<string>? codeFiles);
    IAsyncEnumerable<string?> QueryAsync(string userQuery);
    IList<FileChange> ParseResponseCodeBlocks(string response);
    void ApplyChangesToFiles(IList<FileChange> codeBlocks);
}
