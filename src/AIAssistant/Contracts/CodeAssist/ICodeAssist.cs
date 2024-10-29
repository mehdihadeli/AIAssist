using Clients.Chat.Models;

namespace AIAssistant.Contracts.CodeAssist;

public interface ICodeAssist
{
    Task LoadCodeFiles(ChatSession chatSession, string? contextWorkingDirectory, IList<string>? codeFiles);
    IAsyncEnumerable<string?> QueryAsync(string userQuery);
}
