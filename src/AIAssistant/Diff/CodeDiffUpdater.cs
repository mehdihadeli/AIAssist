using AIAssistant.Contracts.Diff;
using AIAssistant.Models;
using BuildingBlocks.SpectreConsole.Contracts;

namespace AIAssistant.Diff;

public class CodeDiffUpdater(ISpectreUtilities spectreUtilities) : ICodeDiffUpdater
{
    public void ApplyChanges(IEnumerable<FileChange> changes, string contextWorkingDirectory)
    {
        foreach (var change in changes)
        {
            var filePath = Path.Combine(contextWorkingDirectory, change.FilePath);

            try
            {
                // Handle file creation if it's a new file
                if (change.FileCodeChangeType == CodeChangeType.Add)
                {
                    HandleNewFile(change, contextWorkingDirectory);
                    continue;
                }

                // Handle file deletion
                if (change.FileCodeChangeType == CodeChangeType.Delete)
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
                spectreUtilities.Exception($"Failed to update file {filePath}", ex);
            }
        }
    }

    private bool IsFullContentUpdate(FileChange change)
    {
        // If all lines are additions or replacements, treat it as a full content update.
        return change.ChangeLines.All(line =>
            line.LineCodeChangeType == CodeChangeType.Add || line.LineCodeChangeType == CodeChangeType.Update
        );
    }

    private void HandleNewFile(FileChange change, string contextWorkingDirectory)
    {
        var newFileLines = change
            .ChangeLines.Where(line => line.LineCodeChangeType == CodeChangeType.Add)
            .Select(line => line.Content)
            .ToList();

        var directoryPath = Path.GetDirectoryName(change.FilePath)!;
        var fullDirectoryPath = Path.Combine(contextWorkingDirectory, directoryPath);

        if (!string.IsNullOrEmpty(fullDirectoryPath))
        {
            Directory.CreateDirectory(fullDirectoryPath);
        }

        File.WriteAllLines(Path.Combine(contextWorkingDirectory, change.FilePath), newFileLines);
        spectreUtilities.SuccessText($"File created: {change.FilePath}");
    }

    private void HandleFileDeletion(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            spectreUtilities.SuccessText($"File deleted: {filePath}");
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
        spectreUtilities.SuccessText($"File updated: {change.FilePath}");
    }

    private void UpdateLineBasedContent(FileChange change, string filePath)
    {
        if (!File.Exists(filePath))
        {
            spectreUtilities.ErrorText($"File not found: {filePath}");
            return;
        }

        var lines = File.ReadAllLines(filePath).ToList();

        // Process changes in reverse order for proper line indexing
        foreach (var lineChange in change.ChangeLines.OrderByDescending(line => line.LineNumber))
        {
            switch (lineChange.LineCodeChangeType)
            {
                case CodeChangeType.Add:
                    if (lineChange.LineNumber <= lines.Count)
                    {
                        lines.Insert(lineChange.LineNumber - 1, lineChange.Content);
                    }
                    else
                    {
                        lines.Add(lineChange.Content);
                    }
                    break;

                case CodeChangeType.Delete:
                    if (lineChange.LineNumber - 1 < lines.Count)
                    {
                        lines.RemoveAt(lineChange.LineNumber - 1);
                    }
                    break;

                case CodeChangeType.Update:
                    if (lineChange.LineNumber - 1 < lines.Count)
                    {
                        lines[lineChange.LineNumber - 1] = lineChange.Content;
                    }
                    break;
            }
        }

        // Write the updated lines back to the file
        File.WriteAllLines(filePath, lines);
        spectreUtilities.SuccessText($"File updated: {change.FilePath}");
    }
}
