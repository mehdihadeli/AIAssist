using System.Text.RegularExpressions;
using AIAssist.Contracts.CodeAssist;
using AIAssist.Contracts.Diff;
using AIAssist.Models;

namespace AIAssist.Services;

public class CodeAssistantManager(ICodeAssist codeAssist, ICodeDiffManager diffManager) : ICodeAssistantManager
{
    public Task LoadCodeFiles(string contextWorkingDirectory, IList<string>? codeFiles)
    {
        return codeAssist.LoadInitCodeFiles(contextWorkingDirectory, codeFiles);
    }

    public IAsyncEnumerable<string?> QueryAsync(string userQuery)
    {
        return codeAssist.QueryChatCompletionAsync(userQuery);
    }

    public Task AddOrUpdateCodeFiles(IList<string>? codeFiles)
    {
        return codeAssist.AddOrUpdateCodeFiles(codeFiles);
    }

    public Task<IEnumerable<string>> GetCodeTreeContents(IList<string>? codeFiles)
    {
        return codeAssist.GetCodeTreeContents(codeFiles);
    }

    public bool CheckExtraContextForResponse(string response, out IList<string> requiredFiles)
    {
        requiredFiles = new List<string>();

        var pattern = @"Required files for Context:\s*(- .*(?:\r?\n- .*)*)";

        var match = Regex.Match(response, pattern);
        if (match.Success)
        {
            var inlineListContent = match.Groups[1].Value;
            if (!string.IsNullOrEmpty(inlineListContent))
            {
                var lines = inlineListContent.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.StartsWith('-'))
                    {
                        // Remove the leading '-' and trim whitespace
                        requiredFiles.Add(line.TrimStart('-').Trim());
                    }
                }
                return true;
            }
        }

        return false;
    }

    public IList<DiffResult> ParseDiffResults(string diffContent, string contextWorkingDirectory)
    {
        var diffResults = diffManager.ParseDiffResults(diffContent, contextWorkingDirectory);

        return diffResults;
    }

    public void ApplyChanges(IList<DiffResult> diffResults, string contextWorkingDirectory)
    {
        diffManager.ApplyChanges(diffResults, contextWorkingDirectory);
    }
}
