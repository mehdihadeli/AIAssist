using System.Text.RegularExpressions;
using AIAssistant.Contracts;
using AIAssistant.Contracts.Diff;
using AIAssistant.Models;

namespace AIAssistant.Diff;

public class CodeBlockCodeDiffParser : ICodeDiffParser
{
    // regex to match file paths with typical extensions like .cs, .js, etc.
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

            // Detect the start of a new file path
            if (!insideCodeBlock && _filePathRegex.IsMatch(trimmedLine) && !trimmedLine.StartsWith("```"))
            {
                if (currentFilePath != null)
                {
                    // Save the previous code block before starting a new one
                    fileChanges.Add(new FileChange(currentFilePath, string.Join("\n", currentFileContent)));
                }

                // Start a new file block
                currentFilePath = trimmedLine;
                currentFileContent.Clear();
            }
            // Detect the start of a code block
            else if (_codeFenceStartRegex.IsMatch(trimmedLine))
            {
                insideCodeBlock = true;
            }
            // Detect the end of a code block
            else if (_codeFenceEndRegex.IsMatch(trimmedLine) && !string.IsNullOrEmpty(currentFilePath))
            {
                if (insideCodeBlock)
                {
                    insideCodeBlock = false;

                    // Save the current code block
                    fileChanges.Add(new FileChange(currentFilePath, string.Join("\n", currentFileContent)));

                    // Reset for the next potential block
                    currentFilePath = null;
                    currentFileContent.Clear();
                }
            }
            // Accumulate lines inside a code block
            else if (insideCodeBlock)
            {
                currentFileContent.Add(line);
            }
        }

        return fileChanges;
    }
}
