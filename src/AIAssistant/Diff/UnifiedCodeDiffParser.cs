using System.Text.RegularExpressions;
using AIAssistant.Contracts.Diff;
using AIAssistant.Models;

namespace AIAssistant.Diff;

public class UnifiedCodeDiffParser : ICodeDiffParser
{
    private static readonly Regex _hunkHeaderRegex = new(@"^@@.*@@");
    private static readonly string _codeBlockMarker = "```";
    private readonly bool _useHunkHeaderLineNumbers = true;

    public IList<FileChange> GetFileChanges(string diff)
    {
        return ParseUnifiedDiff(diff);
    }

    private IList<FileChange> ParseUnifiedDiff(string diff)
    {
        var fileChanges = new List<FileChange>();
        FileChange? currentFileChange = null;

        var lines = diff.Split('\n');
        bool isFileCreation = false;
        var currentLineNumber = 1;
        bool insideCodeBlock = false;
        bool insideHunk = false;

        //In the context of a unified diff, the hunkOldLineNumber and hunkNewLineNumber are variables used to track the line numbers in the old and new versions of the file
        // Variables to track hunk header line numbers
        int hunkOldLineNumber = 0;
        int hunkNewLineNumber = 0;

        foreach (var codeLine in lines)
        {
            var line = codeLine.TrimEnd(); // Only trim end to preserve indentation

            // Handle code block markers
            if (line.Trim().StartsWith(_codeBlockMarker))
            {
                insideCodeBlock = !insideCodeBlock;
                insideHunk = false;
                continue;
            }

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
                    isFileCreation = true;
                    continue;
                }
                else
                {
                    currentFileChange = new FileChange(filePath, CodeChangeType.Update);
                    isFileCreation = false;
                }
                currentLineNumber = 1;
            }
            else if (line.StartsWith("+++ "))
            {
                var filePath = line.Substring(4).Trim();
                if (filePath == "/dev/null")
                {
                    if (currentFileChange != null)
                    {
                        currentFileChange.FileCodeChangeType = CodeChangeType.Delete;
                    }
                }
                else if (isFileCreation)
                {
                    currentFileChange = new FileChange(filePath, CodeChangeType.Add);
                    isFileCreation = false;
                }
                else if (currentFileChange?.FilePath != filePath)
                {
                    if (currentFileChange != null)
                    {
                        fileChanges.Add(new FileChange(currentFileChange.FilePath, CodeChangeType.Delete));
                    }
                    currentFileChange = new FileChange(filePath, CodeChangeType.Add);
                }
            }
            // A hunk header (contains the line range info for the changes)
            else if (_hunkHeaderRegex.IsMatch(line))
            {
                // Reset line tracking for new block
                insideHunk = true;

                if (_useHunkHeaderLineNumbers)
                {
                    // Extract the line numbers from the hunk header (e.g., `@@ -1,4 +1,4 @@`)
                    var hunkHeader = line.Substring(2, line.IndexOf(" @@", StringComparison.Ordinal) - 2).Trim();
                    var hunkParts = hunkHeader.Split(" ");

                    // remove `-` and `+` from hunk header ranges
                    var oldLineInfo = hunkParts[0].Trim().Substring(1).Split(",");
                    var newLineInfo = hunkParts[1].Trim().Substring(1).Split(",");

                    hunkOldLineNumber = int.Parse(oldLineInfo[0]);
                    hunkNewLineNumber = int.Parse(newLineInfo[0]);
                }

                continue;
            }
            else if (insideHunk)
            {
                // If inside a hunk, process line changes
                if (line.StartsWith("-"))
                {
                    // A line is removed (delete)
                    int lineNumber = _useHunkHeaderLineNumbers ? hunkOldLineNumber++ : currentLineNumber++;
                    // replace `-` with an empty line
                    currentFileChange?.ChangeLines.Add(
                        new FileChangeLine(lineNumber, $"{line.Substring(1)}", CodeChangeType.Delete)
                    );
                }
                else if (line.StartsWith("+"))
                {
                    // A line is added (add)
                    int lineNumber = _useHunkHeaderLineNumbers ? hunkOldLineNumber++ : currentLineNumber++;
                    // replace `+` with an empty line
                    currentFileChange?.ChangeLines.Add(
                        new FileChangeLine(lineNumber, $"{line.Substring(1)}", CodeChangeType.Add)
                    );
                }
                else
                {
                    // Process unchanged or empty lines as well (update)
                    int lineNumber = _useHunkHeaderLineNumbers ? hunkOldLineNumber++ : currentLineNumber++;
                    currentFileChange?.ChangeLines.Add(new FileChangeLine(lineNumber, line, CodeChangeType.Update));
                }
            }
        }

        if (currentFileChange != null)
        {
            fileChanges.Add(currentFileChange);
        }

        return fileChanges;
    }
}
