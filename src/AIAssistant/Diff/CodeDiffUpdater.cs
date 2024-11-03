using AIAssistant.Contracts.Diff;
using AIAssistant.Models;
using BuildingBlocks.SpectreConsole.Contracts;

namespace AIAssistant.Diff;

public class CodeDiffUpdater(ISpectreConsoleUtilities spectreConsoleUtilities) : ICodeDiffUpdater
{
    public void ApplyChanges(IEnumerable<FileChange> changes, string contextWorkingDirectory)
    {
        foreach (var change in changes)
        {
            // Construct the absolute file path based on the context working directory
            var filePath = Path.Combine(contextWorkingDirectory, change.FilePath);

            try
            {
                // Handle file creation if it's a new file
                if (change.IsNewFile)
                {
                    HandleNewFile(change, contextWorkingDirectory);
                    continue;
                }

                // If the file is marked for deletion
                if (change.IsDeletedFile)
                {
                    HandleFileDeletion(filePath);
                    continue;
                }

                // If no line-level changes are specified, assume a full content update
                if (IsFullContentUpdate(change))
                {
                    UpdateFullFileContent(change, contextWorkingDirectory);
                }
                else
                {
                    UpdateLineBasedContent(change);
                }
            }
            catch (Exception ex)
            {
                spectreConsoleUtilities.Exception($"Failed to update file {filePath}", ex);
            }
        }
    }

    private bool IsFullContentUpdate(FileChange change)
    {
        // If the changes are continuous or cover the whole file content, consider it a full content update.
        // Here, we assume if ChangeLines has additions for every line, it's a full update.
        return change.ChangeLines.All(line => line.IsAddition);
    }

    private void HandleNewFile(FileChange change, string contextWorkingDirectory)
    {
        var newFileLines = change.ChangeLines.Where(c => c.IsAddition).Select(c => c.Content).ToList();

        var directoryPath = Path.GetDirectoryName(change.FilePath)!;
        var fullDirectoryPath = Path.Combine(contextWorkingDirectory, directoryPath);
        if (!string.IsNullOrEmpty(fullDirectoryPath))
        {
            Directory.CreateDirectory(fullDirectoryPath);
        }

        File.WriteAllLines(Path.Combine(contextWorkingDirectory, change.FilePath), newFileLines);
        spectreConsoleUtilities.SuccessText($"File created: {change.FilePath}");
    }

    private void HandleFileDeletion(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            spectreConsoleUtilities.SuccessText($"File deleted: {filePath}");
        }
    }

    private void UpdateFullFileContent(FileChange change, string contextWorkingDirectory)
    {
        var fullContent = change.ChangeLines.OrderBy(c => c.LineNumber).Select(c => c.Content).ToList();
        var directoryPath = Path.GetDirectoryName(change.FilePath)!;
        var fullDirectoryPath = Path.Combine(contextWorkingDirectory, directoryPath);

        if (!string.IsNullOrEmpty(fullDirectoryPath))
        {
            Directory.CreateDirectory(fullDirectoryPath);
        }

        File.WriteAllLines(Path.Combine(contextWorkingDirectory, change.FilePath), fullContent);
        spectreConsoleUtilities.SuccessText($"File updated: {change.FilePath}");
    }

    private void UpdateLineBasedContent(FileChange change)
    {
        if (!File.Exists(change.FilePath))
        {
            spectreConsoleUtilities.ErrorText($"File not found: {change.FilePath}");
            return;
        }

        var lines = File.ReadAllLines(change.FilePath).ToList();

        // Process changes in reverse order for proper line indexing
        foreach (var lineChange in change.ChangeLines.OrderByDescending(c => c.LineNumber))
        {
            if (lineChange.IsAddition)
            {
                // Insert the new line
                if (lineChange.LineNumber <= lines.Count)
                {
                    lines.Insert(lineChange.LineNumber - 1, lineChange.Content);
                }
                else
                {
                    // If the line number is beyond the file, add it at the end
                    lines.Add(lineChange.Content);
                }
            }
            else
            {
                // Remove the line if it exists
                if (lineChange.LineNumber - 1 < lines.Count)
                {
                    lines.RemoveAt(lineChange.LineNumber - 1);
                }
            }
        }

        // Write the updated lines back to the file
        File.WriteAllLines(change.FilePath, lines);
        spectreConsoleUtilities.SuccessText($"File updated: {change.FilePath}");
    }
}
