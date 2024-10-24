using AIAssistant.Diff.CodeBlock;
using Clients.Chat.Models;

namespace AIAssistant.Contracts;

public interface ICodeAssistantManager
{
    Task LoadCodeFiles(ChatSession chatSession, string? contextWorkingDirectory, IEnumerable<string>? codeFiles);
    IAsyncEnumerable<string?> QueryAsync(string userQuery);
    IList<CodeBlock> ParseResponseCodeBlocks(string response);
    void ApplyChangesToFiles(IList<CodeBlock> codeBlocks);
}
