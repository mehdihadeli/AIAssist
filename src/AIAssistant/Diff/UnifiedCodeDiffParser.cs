using System.Text.RegularExpressions;
using AIAssistant.Contracts.Diff;
using AIAssistant.Models;

namespace AIAssistant.Diff;

public class UnifiedCodeDiffParser : ICodeDiffParser
{
    private static readonly Regex _diffHeaderRegex = new(@"@@ -(\d+),?(\d+)? \+(\d+),?(\d+)? @@");

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
                // If we already have an ongoing file change, add it to the list
                if (currentFileChange != null)
                {
                    fileChanges.Add(currentFileChange);
                }

                var filePath = line.Substring(4).Trim();
                if (filePath == "/dev/null")
                {
                    // Treat as a deleted file if path is /dev/null
                    currentFileChange = new FileChange(lines[i + 1].Substring(4).Trim(), CodeChangeType.Delete);
                }
                else
                {
                    // New instance for a standard file path
                    currentFileChange = new FileChange(filePath, CodeChangeType.Update);
                }
            }
            else if (line.StartsWith("+++ "))
            {
                continue; // Skip the "+++" line as it is part of the file header
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
                    new FileChangeLine(currentLineNumberOriginal++, line.Substring(1).Trim(), CodeChangeType.Delete)
                );
            }
            // Handle added lines
            else if (line.StartsWith("+"))
            {
                currentFileChange?.ChangeLines.Add(
                    new FileChangeLine(currentLineNumberNew++, line.Substring(1).Trim(), CodeChangeType.Add)
                );
            }
            // Handle unchanged lines, which are usually part of the context
            else
            {
                currentFileChange?.ChangeLines.Add(
                    new FileChangeLine(currentLineNumberOriginal++, line, CodeChangeType.Update)
                );
                currentLineNumberNew++; // Increment both line counters as line is unchanged
            }
        }

        // Add the final file change, if any, to the list
        if (currentFileChange != null)
        {
            fileChanges.Add(currentFileChange);
        }

        return fileChanges;
    }
}
