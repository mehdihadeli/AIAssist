using System.Text.RegularExpressions;
using AIAssistant.Contracts;
using AIAssistant.Contracts.Diff;
using AIAssistant.Models;

namespace AIAssistant.Diff;

public class UnifiedCodeDiffParser : ICodeDiffParser
{
    private static readonly Regex _diffHeaderRegex = new Regex(@"@@ -(\d+),?(\d+)? \+(\d+),?(\d+)? @@");

    public IList<FileChange> ExtractFileChanges(string diff)
    {
        var fileChanges = new List<FileChange>();
        FileChange? currentFileChange = null;

        var lines = diff.Split('\n');
        int currentLineNumberOriginal = 0;
        int currentLineNumberNew = 0;

        bool insideCodeBlock = false;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            // Detect start and end of the code block
            if (line.StartsWith("```diff"))
            {
                insideCodeBlock = true;
                continue;
            }
            else if (line.StartsWith("```"))
            {
                insideCodeBlock = false;
                continue;
            }

            // If not inside a code block, ignore the line
            if (!insideCodeBlock)
                continue;

            // Match the file header
            if (line.StartsWith("--- "))
            {
                if (currentFileChange != null)
                {
                    fileChanges.Add(currentFileChange);
                }
                var filePath = line.Substring(4).Trim();

                if (filePath == "/dev/null")
                {
                    currentFileChange = new FileChange(lines[i + 1].Substring(4).Trim()) { IsNewFile = true };
                }
                else
                {
                    currentFileChange = new FileChange(filePath);
                }
            }
            else if (line.StartsWith("+++ "))
            {
                continue; // Skip the "+++" line as it's handled in the previous step
            }
            // Match the diff headers with line numbers
            else if (line.StartsWith("@@"))
            {
                var match = _diffHeaderRegex.Match(line);
                if (match.Success)
                {
                    currentLineNumberOriginal = int.Parse(match.Groups[1].Value);
                    currentLineNumberNew = int.Parse(match.Groups[3].Value);
                }
            }
            // Handle removed lines
            else if (line.StartsWith("-"))
            {
                currentFileChange?.ChangeLines.Add(
                    new FileChangeLine(currentLineNumberOriginal++, line.Substring(1).Trim(), false)
                );
            }
            // Handle added lines
            else if (line.StartsWith("+"))
            {
                currentFileChange?.ChangeLines.Add(
                    new FileChangeLine(currentLineNumberNew++, line.Substring(1).Trim(), true)
                );
            }
        }

        if (currentFileChange != null)
        {
            fileChanges.Add(currentFileChange);
        }

        return fileChanges;
    }
}
