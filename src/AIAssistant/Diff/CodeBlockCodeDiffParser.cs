using System.Text.RegularExpressions;
using AIAssistant.Contracts;
using AIAssistant.Contracts.Diff;
using AIAssistant.Models;

namespace AIAssistant.Diff;

public class CodeBlockCodeDiffParser : ICodeDiffParser
{
    private static readonly Regex _filePathRegex = new(@"^[\w\-./\\]+?\.[\w]+$", RegexOptions.Compiled);
    private static readonly Regex _codeFenceStartRegex = new(@"^```csharp$", RegexOptions.Compiled);
    private static readonly Regex _codeFenceEndRegex = new(@"^```$", RegexOptions.Compiled);

    public IList<FileChange> ExtractFileChanges(string diff)
    {
        var fileChanges = new List<FileChange>();
        string? currentFilePath = null;
        var currentFileContent = new List<string>();
        bool insideCodeBlock = false;

        var lines = diff.Split('\n');

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (!insideCodeBlock && _filePathRegex.IsMatch(trimmedLine) && !trimmedLine.StartsWith("```"))
            {
                if (currentFilePath != null)
                {
                    fileChanges.Add(
                        new FileChange(currentFilePath, string.Join("\n", currentFileContent), ChangeType.Update)
                    );
                }

                currentFilePath = trimmedLine;
                currentFileContent.Clear();
            }
            else if (_codeFenceStartRegex.IsMatch(trimmedLine))
            {
                insideCodeBlock = true;
            }
            else if (_codeFenceEndRegex.IsMatch(trimmedLine) && !string.IsNullOrEmpty(currentFilePath))
            {
                if (insideCodeBlock)
                {
                    insideCodeBlock = false;
                    fileChanges.Add(
                        new FileChange(currentFilePath, string.Join("\n", currentFileContent), ChangeType.Update)
                    );
                    currentFilePath = null;
                    currentFileContent.Clear();
                }
            }
            else if (insideCodeBlock)
            {
                currentFileContent.Add(line);
            }
        }

        return fileChanges;
    }
}
