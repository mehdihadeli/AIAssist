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
            var filePath = Path.Combine(contextWorkingDirectory, change.FilePath);

            try
            {
                // Handle file creation if it's a new file
                if (change.FileChangeType == ChangeType.Add)
                {
                    HandleNewFile(change, contextWorkingDirectory);
                    continue;
                }

                // Handle file deletion
                if (change.FileChangeType == ChangeType.Delete)
                {
                    HandleFileDeletion(filePath);
                    continue;
                }

                // Handle file updates based on line-level changes
                if (IsFullContentUpdate(change))
                {
                    UpdateFullFileContent(change, contextWorkingDirectory);
                }
                else
                {
                    UpdateLineBasedContent(change, filePath);
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
        // If all lines are additions or replacements, treat it as a full content update.
        return change.ChangeLines.All(line =>
            line.LineChangeType == ChangeType.Add || line.LineChangeType == ChangeType.Update
        );
    }

    private void HandleNewFile(FileChange change, string contextWorkingDirectory)
    {
        var newFileLines = change
            .ChangeLines.Where(line => line.LineChangeType == ChangeType.Add)
            .Select(line => line.Content)
            .ToList();

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
        var fullContent = change.ChangeLines.OrderBy(line => line.LineNumber).Select(line => line.Content).ToList();

        var directoryPath = Path.GetDirectoryName(change.FilePath)!;
        var fullDirectoryPath = Path.Combine(contextWorkingDirectory, directoryPath);

        if (!string.IsNullOrEmpty(fullDirectoryPath))
        {
            Directory.CreateDirectory(fullDirectoryPath);
        }

        File.WriteAllLines(Path.Combine(contextWorkingDirectory, change.FilePath), fullContent);
        spectreConsoleUtilities.SuccessText($"File updated: {change.FilePath}");
    }

    private void UpdateLineBasedContent(FileChange change, string filePath)
    {
        if (!File.Exists(filePath))
        {
            spectreConsoleUtilities.ErrorText($"File not found: {filePath}");
            return;
        }

        var lines = File.ReadAllLines(filePath).ToList();

        // Process changes in reverse order for proper line indexing
        foreach (var lineChange in change.ChangeLines.OrderByDescending(line => line.LineNumber))
        {
            switch (lineChange.LineChangeType)
            {
                case ChangeType.Add:
                    if (lineChange.LineNumber <= lines.Count)
                    {
                        lines.Insert(lineChange.LineNumber - 1, lineChange.Content);
                    }
                    else
                    {
                        lines.Add(lineChange.Content);
                    }
                    break;

                case ChangeType.Delete:
                    if (lineChange.LineNumber - 1 < lines.Count)
                    {
                        lines.RemoveAt(lineChange.LineNumber - 1);
                    }
                    break;

                case ChangeType.Update:
                    if (lineChange.LineNumber - 1 < lines.Count)
                    {
                        lines[lineChange.LineNumber - 1] = lineChange.Content;
                    }
                    break;
            }
        }

        // Write the updated lines back to the file
        File.WriteAllLines(filePath, lines);
        spectreConsoleUtilities.SuccessText($"File updated: {change.FilePath}");
    }
}
