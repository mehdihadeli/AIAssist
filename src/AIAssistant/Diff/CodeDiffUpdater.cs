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
                switch (change.FileCodeChangeType)
                {
                    case CodeChangeType.Add:
                        HandleNewFile(change, contextWorkingDirectory);
                        break;
                    case CodeChangeType.Delete:
                        HandleFileDeletion(filePath);
                        break;
                    case CodeChangeType.Update:
                        HandleFileUpdate(change, contextWorkingDirectory);
                        break;
                }
            }
            catch (Exception ex)
            {
                spectreUtilities.Exception($"Failed to update file {filePath}", ex);
            }
        }
    }

    private void HandleNewFile(FileChange change, string contextWorkingDirectory)
    {
        var content = change.ChangeLines.OrderBy(l => l.LineNumber).Select(l => l.Content).ToList();
        var fullPath = Path.Combine(contextWorkingDirectory, change.FilePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllLines(fullPath, content);
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

    private void HandleFileUpdate(FileChange fileChange, string contextWorkingDirectory)
    {
        var filePath = Path.Combine(contextWorkingDirectory, fileChange.FilePath);

        // For complete file replacements (all changes are new)
        if (fileChange.ChangeLines.All(l => l.LineCodeChangeType != CodeChangeType.Update))
        {
            var content = fileChange
                .ChangeLines.Where(l => l.LineCodeChangeType == CodeChangeType.Add)
                .OrderBy(l => l.LineNumber)
                .Select(l => l.Content)
                .ToList();

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllLines(filePath, content);
            spectreUtilities.SuccessText($"File replaced: {fileChange.FilePath}");
            return;
        }

        // For partial updates
        var originalLines = File.Exists(filePath) ? File.ReadAllLines(filePath).ToList() : new List<string>();
        var updatedLines = new List<string>();
        var currentOriginalIndex = 0;

        var changesByLine = fileChange
            .ChangeLines.OrderBy(l => l.LineNumber)
            .GroupBy(l => l.LineNumber)
            .ToDictionary(g => g.Key, g => g.ToList());

        var maxLineNumber = changesByLine.Keys.Max();

        for (int lineNumber = 1; lineNumber <= maxLineNumber; lineNumber++)
        {
            if (changesByLine.TryGetValue(lineNumber, out var changes))
            {
                foreach (var change in changes)
                {
                    switch (change.LineCodeChangeType)
                    {
                        case CodeChangeType.Add:
                            updatedLines.Add(change.Content);
                            break;
                        case CodeChangeType.Delete:
                            if (currentOriginalIndex < originalLines.Count)
                            {
                                currentOriginalIndex++;
                            }
                            break;
                        case CodeChangeType.Update:
                            if (currentOriginalIndex < originalLines.Count)
                            {
                                updatedLines.Add(change.Content);
                            }
                            currentOriginalIndex++;
                            break;
                    }
                }
            }
            else if (currentOriginalIndex < originalLines.Count)
            {
                updatedLines.Add(originalLines[currentOriginalIndex++]);
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllLines(filePath, updatedLines);
        spectreUtilities.SuccessText($"File updated: {fileChange.FilePath}");
    }
}
