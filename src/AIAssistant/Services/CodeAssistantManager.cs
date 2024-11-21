using System.Collections;
using System.Text.RegularExpressions;
using AIAssistant.Contracts.CodeAssist;
using AIAssistant.Contracts.Diff;
using AIAssistant.Models;

namespace AIAssistant.Services;

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

    public Task AddOrUpdateCodeFilesToCache(IList<string>? codeFiles)
    {
        return codeAssist.AddOrUpdateCodeFiles(codeFiles);
    }

    public Task<IEnumerable<string>> GetCodeTreeContentsFromCache(IList<string>? codeFiles)
    {
        return codeAssist.GetCodeTreeContents(codeFiles);
    }

    public bool CheckExtraContextForResponse(string response, out IList<string> requiredFiles)
    {
        requiredFiles = new List<string>();
        var pattern = @"Required Files for Context:\s*(?:```[\w]*\s*([\s\S]*?)\s*```|((?:- .*\r?\n?)+))";

        var match = Regex.Match(response, pattern);
        if (match.Success)
        {
            // Check for code block content first
            var codeBlockContent = match.Groups[1].Value;
            if (!string.IsNullOrEmpty(codeBlockContent))
            {
                var lines = codeBlockContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    requiredFiles.Add(line.TrimStart('-').Trim());
                }
                return true;
            }

            // If no code block content, check for inline list
            var inlineListContent = match.Groups[2].Value;
            if (!string.IsNullOrEmpty(inlineListContent))
            {
                var lines = inlineListContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    requiredFiles.Add(line.TrimStart('-').Trim());
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
