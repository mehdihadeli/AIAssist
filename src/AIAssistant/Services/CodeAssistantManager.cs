using AIAssistant.Contracts.CodeAssist;
using AIAssistant.Contracts.Diff;
using AIAssistant.Models;

namespace AIAssistant.Services;

public class CodeAssistantManager(ICodeAssist codeAssist, ICodeDiffManager diffManager) : ICodeAssistantManager
{
    public Task LoadCodeFiles(string contextWorkingDirectory, IList<string>? codeFiles)
    {
        return codeAssist.LoadCodeFiles(contextWorkingDirectory, codeFiles);
    }

    public IAsyncEnumerable<string?> QueryAsync(string userQuery)
    {
        return codeAssist.QueryChatCompletionAsync(userQuery);
    }

    public IList<FileChange> ParseResponseCodeBlocks(string response)
    {
        var codeBlocks = diffManager.ExtractFileChanges(response);

        return codeBlocks;
    }

    public void ApplyChangesToFiles(IList<FileChange> codeBlocks, string contextWorkingDirectory)
    {
        diffManager.ApplyChanges(codeBlocks, contextWorkingDirectory);
    }
}
