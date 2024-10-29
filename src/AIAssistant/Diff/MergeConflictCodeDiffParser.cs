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

    public IList<FileChange> ExtractFileChanges(string diff)
    {
        var changes = new List<FileChange>();
        string? currentFilePath = null;
        var changeLines = new List<FileChangeLine>();
        bool isPreviousVersion = false;
        bool isNewVersion = false;
        int currentLineNumber = 1;
        int originalLineNumber = 1;
        bool isNewFile = false;
        bool isDeletedFile = false;

        var lines = diff.Split('\n');

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Detect the file path line
            if (_filePathRegex.IsMatch(trimmedLine))
            {
                // Save the previous file change if applicable
                if (currentFilePath != null && changeLines.Count != 0)
                {
                    changes.Add(
                        new FileChange(currentFilePath, isNewFile, isDeletedFile) { ChangeLines = changeLines.ToList() }
                    );
                    changeLines.Clear();
                }

                currentFilePath = trimmedLine;
                currentLineNumber = 1;
                originalLineNumber = 1;
                isNewFile = false;
                isDeletedFile = false;
                continue;
            }

            // Detect the start of the previous version
            if (_previousVersionStart.IsMatch(trimmedLine))
            {
                isPreviousVersion = true;
                isNewVersion = false;
                continue;
            }

            // Detect the separator between previous and new versions
            if (_separator.IsMatch(trimmedLine))
            {
                isPreviousVersion = false;
                isNewVersion = true;
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
                // Track if it's a deleted file
                isDeletedFile = true;
                changeLines.Add(new FileChangeLine(originalLineNumber++, trimmedLine, isAddition: false));
            }
            else if (isNewVersion)
            {
                // Track if it's a new file
                isNewFile = true;
                changeLines.Add(new FileChangeLine(currentLineNumber++, trimmedLine, isAddition: true));
            }
        }

        // Save the last file change if applicable
        if (currentFilePath != null && changeLines.Count != 0)
        {
            changes.Add(
                new FileChange(currentFilePath, isNewFile, isDeletedFile) { ChangeLines = changeLines.ToList() }
            );
        }

        return changes;
    }
}
