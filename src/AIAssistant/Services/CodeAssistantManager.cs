using AIAssistant.Contracts;
using AIAssistant.Contracts.CodeAssist;
using AIAssistant.Contracts.Diff;
using AIAssistant.Models;
using Clients.Chat.Models;

namespace AIAssistant.Services;

public class CodeAssistantManager(ICodeAssist codeAssist, ICodeDiffManager diffManager) : ICodeAssistantManager
{
    public Task LoadCodeFiles(ChatSession chatSession, string? contextWorkingDirectory, IList<string>? codeFiles)
    {
        return codeAssist.LoadCodeFiles(chatSession, contextWorkingDirectory, codeFiles);
    }

    public IAsyncEnumerable<string?> QueryAsync(string userQuery)
    {
        return codeAssist.QueryAsync(userQuery);
    }

    public IList<FileChange> ParseResponseCodeBlocks(string response)
    {
        var codeBlocks = diffManager.ExtractFileChanges(response);

        return codeBlocks;
    }

    public void ApplyChangesToFiles(IList<FileChange> codeBlocks)
    {
        diffManager.ApplyChanges(codeBlocks);
    }
}
