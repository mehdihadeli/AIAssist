using System.Text.RegularExpressions;
using AIAssistant.Contracts;
using AIAssistant.Contracts.Diff;
using AIAssistant.Models;

namespace AIAssistant.Diff;

public class MergeConflictCodeDiffParser : ICodeDiffParser
{
    private static readonly Regex _filePathRegex = new(@"^([\w\-./\\]+?\.[\w]+)$", RegexOptions.Compiled);
    private static readonly Regex _previousVersionStart = new(@"^<<<<<<< PREVIOUS VERSION$", RegexOptions.Compiled);
    private static readonly Regex _newVersionEnd = new(@"^>>>>>>> NEW VERSION$", RegexOptions.Compiled);
    private static readonly Regex _separator = new(@"^=======$", RegexOptions.Compiled);

    public IList<FileChange> GetFileChanges(string diff)
    {
        var changes = new List<FileChange>();
        string? currentFilePath = null;
        var changeLines = new List<FileChangeLine>();
        bool isPreviousVersion = false;
        bool isNewVersion = false;
        int currentLineNumber = 1;
        int originalLineNumber = 1;
        CodeChangeType fileCodeChangeType = CodeChangeType.Update;

        var lines = diff.Split('\n');

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (_filePathRegex.IsMatch(trimmedLine))
            {
                // Save the previous file change if applicable
                if (currentFilePath != null && changeLines.Count != 0)
                {
                    changes.Add(new FileChange(currentFilePath, fileCodeChangeType, changeLines.ToList()));
                    changeLines.Clear();
                }

                currentFilePath = trimmedLine;
                currentLineNumber = 1;
                originalLineNumber = 1;
                fileCodeChangeType = CodeChangeType.Update;
                continue;
            }

            // Detect the start of the previous version
            if (_previousVersionStart.IsMatch(trimmedLine))
            {
                isPreviousVersion = true;
                isNewVersion = false;
                fileCodeChangeType = CodeChangeType.Delete; // Considered a deletion in conflicts
                continue;
            }

            // Detect the separator between previous and new versions
            if (_separator.IsMatch(trimmedLine))
            {
                isPreviousVersion = false;
                isNewVersion = true;
                fileCodeChangeType = CodeChangeType.Add; // Considered an addition in conflicts
                continue;
            }

            // Detect the end of the new version
            if (_newVersionEnd.IsMatch(trimmedLine))
            {
                isNewVersion = false;
                continue;
            }

            // Accumulate lines as previous or new version changes
            if (isPreviousVersion)
            {
                changeLines.Add(new FileChangeLine(originalLineNumber++, trimmedLine, CodeChangeType.Delete));
            }
            else if (isNewVersion)
            {
                changeLines.Add(new FileChangeLine(currentLineNumber++, trimmedLine, CodeChangeType.Add));
            }
        }

        // Save the last file change if applicable
        if (currentFilePath != null && changeLines.Count != 0)
        {
            changes.Add(new FileChange(currentFilePath, fileCodeChangeType, changeLines.ToList()));
        }

        return changes;
    }
}
