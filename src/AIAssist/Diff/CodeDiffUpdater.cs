using AIAssist.Contracts.Diff;
using AIAssist.Models;
using BuildingBlocks.SpectreConsole.Contracts;

namespace AIAssist.Diff;

public class CodeDiffUpdater(ISpectreUtilities spectreUtilities) : ICodeDiffUpdater
{
    public void ApplyChanges(IList<DiffResult> diffResults, string contextWorkingDirectory)
    {
        ArgumentNullException.ThrowIfNull(diffResults);

        if (string.IsNullOrWhiteSpace(contextWorkingDirectory))
        {
            spectreUtilities.ErrorTextLine("Working directory cannot be null or whitespace.");
        }

        foreach (var diffResult in diffResults)
        {
            if (diffResult.Replacements == null)
            {
                HandleCodeChange(diffResult, contextWorkingDirectory);
                return;
            }

            HandleReplacementFile(diffResult, contextWorkingDirectory);
        }
    }

    private void HandleCodeChange(DiffResult diffResult, string contextWorkingDirectory)
    {
        ApplyFileChanges(
            diffResult.ModifiedLines,
            diffResult.Action,
            diffResult.OriginalPath,
            diffResult.ModifiedPath,
            contextWorkingDirectory
        );
    }

    private void HandleReplacementFile(DiffResult diffResult, string contextWorkingDirectory)
    {
        var noneExistPath = "/dev/null";

        if (diffResult.OriginalPath == noneExistPath && diffResult.ModifiedPath != noneExistPath)
        {
            // New file creation
            var modifiedFilePath = Path.Combine(contextWorkingDirectory, diffResult.ModifiedPath);

            Directory.CreateDirectory(Path.GetDirectoryName(modifiedFilePath)!);

            if (diffResult.Replacements is not null && diffResult.Replacements.Any())
            {
                var updatedLines = ApplyReplacements(new List<string>(), diffResult.Replacements);
                File.WriteAllText(modifiedFilePath, string.Join("\n", updatedLines));
                spectreUtilities.SuccessTextLine($"File created: {modifiedFilePath}");
            }
            else
            {
                spectreUtilities.ErrorTextLine("No modified lines provided for new file creation.");
            }
        }
        else if (diffResult.ModifiedPath == noneExistPath && diffResult.OriginalPath != noneExistPath)
        {
            // File deletion
            var originalFilePath = Path.Combine(contextWorkingDirectory, diffResult.OriginalPath);

            if (File.Exists(originalFilePath) && diffResult.Replacements is not null && diffResult.Replacements.Any())
            {
                File.Delete(originalFilePath);
                spectreUtilities.SuccessTextLine($"File deleted: {originalFilePath}");
            }
            else
            {
                spectreUtilities.ErrorTextLine($"File not found for deletion: {originalFilePath}");
            }
        }
        else if (diffResult.OriginalPath != diffResult.ModifiedPath)
        {
            // File rename or move
            var originalFilePath = Path.Combine(contextWorkingDirectory, diffResult.OriginalPath);
            var modifiedFilePath = Path.Combine(contextWorkingDirectory, diffResult.ModifiedPath);

            if (File.Exists(originalFilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(modifiedFilePath)!);
                File.Move(originalFilePath, modifiedFilePath, overwrite: true);
                if (diffResult.Replacements is not null && diffResult.Replacements.Any())
                {
                    var modifiedFileLines = File.ReadAllLines(modifiedFilePath).ToList();
                    var updatedLines = ApplyReplacements(modifiedFileLines, diffResult.Replacements);
                    File.WriteAllText(modifiedFilePath, string.Join("\n", updatedLines));
                }
            }
            else
            {
                spectreUtilities.ErrorTextLine($"Original file not found for rename/move: {originalFilePath}");
            }
        }
        else
        {
            // File update
            if (diffResult.Replacements is null || !diffResult.Replacements.Any())
                return;

            var originalFilePath = Path.Combine(contextWorkingDirectory, diffResult.OriginalPath);
            var modifiedFilePath = Path.Combine(contextWorkingDirectory, diffResult.ModifiedPath);

            if (!File.Exists(originalFilePath))
            {
                spectreUtilities.ErrorTextLine($"Original file not found: {originalFilePath}");
            }

            var originalLines = File.ReadAllLines(originalFilePath).ToList();
            var updatedLines = ApplyReplacements(originalLines, diffResult.Replacements);

            Directory.CreateDirectory(Path.GetDirectoryName(modifiedFilePath)!);
            File.WriteAllText(modifiedFilePath, string.Join("\n", updatedLines));

            spectreUtilities.SuccessTextLine($"File updated: {modifiedFilePath}");
        }
    }

    private List<string> ApplyReplacements(List<string> originalLines, IList<Replacement> replacements)
    {
        var updatedLines = new List<string>(originalLines);

        // Sort replacements in descending order to avoid index shifting issues
        var sortedReplacements = replacements.OrderByDescending(r => r.OriginalFileStartIndex).ToList();

        foreach (var replacement in sortedReplacements)
        {
            // Remove the range of lines specified by StartIndex and EndIndex
            if (replacement.OriginalFileEndIndex > replacement.OriginalFileStartIndex)
            {
                updatedLines.RemoveRange(
                    replacement.OriginalFileStartIndex,
                    replacement.OriginalFileEndIndex - replacement.OriginalFileStartIndex
                );
            }

            // Insert the new lines at the StartIndex position
            updatedLines.InsertRange(replacement.OriginalFileStartIndex, replacement.NewLines);
        }

        return updatedLines;
    }

    private void ApplyFileChanges(
        IEnumerable<string>? modifiedLines,
        ActionType actionType,
        string originalPath,
        string modifiedPath,
        string contextWorkingDirectory
    )
    {
        if (modifiedLines is null)
            return;

        try
        {
            switch (actionType)
            {
                case ActionType.Add:
                {
                    var modifiedFullPath = Path.Combine(contextWorkingDirectory, modifiedPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(modifiedFullPath)!);
                    // Normalize and write lines to prevent extra blank lines because WriteAllLines
                    File.WriteAllText(modifiedFullPath, string.Join("\n", modifiedLines));
                    spectreUtilities.SuccessTextLine($"File created: {modifiedPath}");

                    break;
                }

                case ActionType.Update:
                {
                    var modifiedFullPath = Path.Combine(contextWorkingDirectory, modifiedPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(modifiedFullPath)!);
                    if (File.Exists(modifiedFullPath))
                    {
                        // Normalize and write lines to prevent blank lines
                        File.WriteAllText(modifiedFullPath, string.Join("\n", modifiedLines));
                        spectreUtilities.SuccessTextLine($"File updated: {modifiedPath}");
                    }
                    else
                    {
                        spectreUtilities.ErrorTextLine($"File {modifiedPath} does not exist to modify.");
                    }
                    break;
                }

                case ActionType.Delete:
                {
                    var originalFullPath = Path.Combine(contextWorkingDirectory, originalPath);
                    if (File.Exists(originalFullPath))
                    {
                        File.Delete(originalFullPath);
                        spectreUtilities.SuccessTextLine($"File deleted: {originalPath}");
                    }
                    else
                    {
                        spectreUtilities.ErrorTextLine($"File {originalPath} not found for deletion.");
                    }
                    break;
                }

                default:
                    spectreUtilities.ErrorTextLine($"Unsupported action type: {actionType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            spectreUtilities.ErrorTextLine($"Failed to update file {modifiedPath} \n {ex.Message}");
        }
    }
}
