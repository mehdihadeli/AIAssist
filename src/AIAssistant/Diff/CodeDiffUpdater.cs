using AIAssistant.Contracts;
using AIAssistant.Contracts.Diff;
using AIAssistant.Models;
using Spectre.Console;

namespace AIAssistant.Diff;

public class CodeDiffUpdater : ICodeDiffUpdater
{
    public void ApplyChanges(IEnumerable<FileChange> changes)
    {
        foreach (var change in changes)
        {
            var filePath = change.FilePath;

            try
            {
                // Handle file creation if it's a new file
                if (change.IsNewFile)
                {
                    HandleNewFile(change);
                    continue;
                }

                // If the file is marked for deletion
                if (change.IsDeletedFile)
                {
                    HandleFileDeletion(change);
                    continue;
                }

                // If no line-level changes are specified, assume a full content update
                if (IsFullContentUpdate(change))
                {
                    UpdateFullFileContent(change);
                }
                else
                {
                    UpdateLineBasedContent(change);
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Failed to update file {filePath}: {ex.Message}[/]");
            }
        }
    }

    private bool IsFullContentUpdate(FileChange change)
    {
        // If the changes are continuous or cover the whole file content, consider it a full content update.
        // Here, we assume if ChangeLines has additions for every line, it's a full update.
        return change.ChangeLines.All(line => line.IsAddition);
    }

    private void HandleNewFile(FileChange change)
    {
        var newFileLines = change.ChangeLines.Where(c => c.IsAddition).Select(c => c.Content).ToList();

        var directoryPath = Path.GetDirectoryName(change.FilePath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.WriteAllLines(change.FilePath, newFileLines);
        AnsiConsole.MarkupLine($"[green]File created: {change.FilePath}[/]");
    }

    private void HandleFileDeletion(FileChange change)
    {
        if (File.Exists(change.FilePath))
        {
            File.Delete(change.FilePath);
            AnsiConsole.MarkupLine($"[yellow]Deleted file: {change.FilePath}[/]");
        }
    }

    private void UpdateFullFileContent(FileChange change)
    {
        var fullContent = change.ChangeLines.OrderBy(c => c.LineNumber).Select(c => c.Content).ToList();

        var directoryPath = Path.GetDirectoryName(change.FilePath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.WriteAllLines(change.FilePath, fullContent);
        AnsiConsole.MarkupLine($"[green]Full file content updated: {change.FilePath}[/]");
    }

    private void UpdateLineBasedContent(FileChange change)
    {
        if (!File.Exists(change.FilePath))
        {
            AnsiConsole.MarkupLine($"[red]File not found: {change.FilePath}[/]");
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
        AnsiConsole.MarkupLine($"[green]File updated: {change.FilePath}[/]");
    }
}
