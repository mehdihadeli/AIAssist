using Clients.Chat.Models;
using Clients.Models;

namespace AIAssistant.Contracts;

public interface ICodeStrategy
{
    Task LoadCodeFiles(ChatSession chatSession, string? contextWorkingDirectory, IEnumerable<string>? codeFiles);
    IAsyncEnumerable<string?> QueryAsync(string userQuery);
}
