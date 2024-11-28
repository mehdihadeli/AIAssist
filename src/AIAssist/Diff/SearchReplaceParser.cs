using AIAssist.Contracts.Diff;
using AIAssist.Models;
using BuildingBlocks.Utils;

namespace AIAssist.Diff;

public class SearchReplaceParser : ICodeDiffParser
{
    private readonly string _originalFileHunkMarker = "---";
    private readonly string _modifiedFileHunkMarker = "+++";
    private readonly string _hunkHeaderMarker = "@@ @@";

    public IList<DiffResult> ParseDiffResults(string diffContent, string contextWorkingDirectory)
    {
        // Extract all code blocks enclosed in backticks
        var codeBlocks = ExtractMarkdownCodeBlocks(diffContent);
        var diffResults = new List<DiffResult>();

        foreach (var codeBlock in codeBlocks)
        {
            var (hunks, originalPath, modifiedPath) = ExtractHunks(codeBlock);

            var replacements = ProcessHunks(hunks, originalPath, modifiedPath, contextWorkingDirectory);

            diffResults.Add(
                new DiffResult
                {
                    Replacements = replacements,
                    OriginalPath = originalPath,
                    ModifiedPath = modifiedPath,
                    Action = GetAction(originalPath, modifiedPath),
                    ModifiedLines = null,
                }
            );
        }

        return diffResults;
    }

    private static ActionType GetAction(string originalPath, string modifiedPath)
    {
        var noneExistPath = "/dev/null";
        if (
            !string.IsNullOrEmpty(originalPath)
            && originalPath == noneExistPath
            && !string.IsNullOrEmpty(modifiedPath)
            && modifiedPath != noneExistPath
        )
        {
            return ActionType.Add;
        }
        if (
            !string.IsNullOrEmpty(modifiedPath)
            && modifiedPath == noneExistPath
            && !string.IsNullOrEmpty(originalPath)
            && originalPath != noneExistPath
        )
        {
            return ActionType.Delete;
        }

        return ActionType.Update;
    }

    private static IList<Replacement> ProcessHunks(
        List<Hunk> hunks,
        string originalPath,
        string modifiedPath,
        string contextWorkingDirectory
    )
    {
        const string devNull = "/dev/null";
        var replacements = new List<Replacement>();

        // Handle addition (originalPath == /dev/null)
        if (originalPath == devNull)
        {
            foreach (var hunk in hunks)
            {
                // there is no delete or unchanged
                if (ExtractSearchLines(hunk).Count == 0)
                {
                    var addedLines = ExtractNewLines(hunk);
                    hunk.OriginalStart = 0;
                    hunk.OriginalCount = 0;
                    hunk.ModifiedStart = 0; // Assuming this is the start of the file
                    hunk.ModifiedCount = addedLines.Count;

                    // In the case of an addition where the original file path is /dev/null, the endIndex is not particularly meaningful because the operation does not involve removing or replacing any existing lines, so we set it to 0
                    replacements.Add(new Replacement(0, 0, addedLines)); // Add all new lines at the beginning
                }
            }
            return replacements;
        }

        // Handle deletion (modifiedPath == /dev/null)
        if (modifiedPath == devNull)
        {
            foreach (var hunk in hunks)
            {
                var deletedLines = ExtractSearchLines(hunk);
                // Use startIndex as 0 (start of file) and endIndex as the count of deleted lines for removing all lines
                replacements.Add(new Replacement(0, deletedLines.Count, new List<string>()));
            }

            return replacements;
        }

        // Handle modification
        var originalFullFilePath = Path.Combine(contextWorkingDirectory, originalPath);
        if (!File.Exists(originalFullFilePath))
        {
            throw new FileNotFoundException($"Original file not found: {originalFullFilePath}");
        }

        var originalLines = File.ReadAllLines(originalFullFilePath);

        foreach (var hunk in hunks)
        {
            if (hunk.HunkItems.Count == 0)
                continue;

            var searchLines = ExtractSearchLines(hunk);
            if (searchLines.Count == 0)
            {
                continue;
            }

            // index starts from 0
            int startIndex = MatchUtilities.FindMatchIndex(originalLines, searchLines);
            if (startIndex == -1)
                continue;

            // Align empty lines in the original file and hunk for matching context
            AlignEmptyLines(hunk, originalLines, startIndex);

            hunk.OriginalStart = startIndex;
            hunk.OriginalCount = hunk.HunkItems.Count(item => item.ChangeType != ChangeType.Add);
            hunk.ModifiedStart = startIndex;
            hunk.ModifiedCount = hunk.HunkItems.Count(item => item.ChangeType != ChangeType.Delete);

            replacements.AddRange(BuildReplacements(hunk, startIndex));
        }

        return replacements;
    }

    private static IEnumerable<Replacement> BuildReplacements(Hunk hunk, int startIndex)
    {
        var replacements = new List<Replacement>();
        int currentStart = -1;
        var currentAdditions = new List<string>();
        int currentIndex = startIndex;

        foreach (var hunkItem in hunk.HunkItems)
        {
            if (hunkItem.ChangeType == ChangeType.Unchanged || string.IsNullOrWhiteSpace(hunkItem.Line))
            {
                // Finalize any pending additions when we reach context line
                if (currentStart != -1)
                {
                    replacements.Add(new Replacement(currentStart, currentIndex, new List<string>(currentAdditions)));
                    currentAdditions.Clear();
                    currentStart = -1;
                }
                currentIndex++;
            }
            else if (hunkItem.ChangeType == ChangeType.Add)
            {
                if (currentStart == -1)
                    currentStart = currentIndex;
                currentAdditions.Add(hunkItem.Line.Substring(1));
            }
            else if (hunkItem.ChangeType == ChangeType.Delete)
            {
                if (currentStart == -1)
                    currentStart = currentIndex;
                currentIndex++;
            }
        }

        // Capture any remaining additions at the end of the change hunk
        if (currentStart != -1)
        {
            replacements.Add(new Replacement(currentStart, currentIndex, currentAdditions));
        }

        return replacements;
    }

    private static void AlignEmptyLines(Hunk hunk, IList<string> originalLines, int startIndex)
    {
        int currentFileIndex = startIndex;
        int currentChangeIndex = 0;
        while (currentChangeIndex < hunk.HunkItems.Count && currentFileIndex < originalLines.Count)
        {
            var curChangeLine = hunk.HunkItems[currentChangeIndex];

            if (curChangeLine.ChangeType == ChangeType.Add)
            {
                currentChangeIndex++;
                continue;
            }

            var currentFileLine = originalLines[currentFileIndex];

            // Align empty lines for matching context
            if (currentFileLine.Trim().Length == 0 && curChangeLine.Line.Trim().Length != 0)
            {
                hunk.HunkItems.Insert(currentChangeIndex, new DiffHunkItem(ChangeType.Unchanged, ""));
            }
            else if (curChangeLine.Line.Trim().Length == 0 && currentFileLine.Trim().Length != 0)
            {
                originalLines.Insert(currentFileIndex, "");
            }

            currentFileIndex++;
            currentChangeIndex++;
        }
    }

    private (List<Hunk> Hunks, string OriginalPath, string ModifiedPath) ExtractHunks(string codeBlock)
    {
        var lines = codeBlock.Split('\n');
        var hunks = new List<Hunk>();
        Hunk? currentHunk = null;
        bool insideHunk = false;

        string originalPath = string.Empty;
        string modifiedPath = string.Empty;

        List<string> tempEmptyLines = new List<string>();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (trimmedLine.StartsWith(_originalFileHunkMarker))
            {
                originalPath = trimmedLine.Substring(_originalFileHunkMarker.Length).Trim();
            }
            else if (trimmedLine.StartsWith(_modifiedFileHunkMarker))
            {
                modifiedPath = trimmedLine.Substring(_modifiedFileHunkMarker.Length).Trim();
            }
            else if (trimmedLine == _hunkHeaderMarker)
            {
                // Start a new hunk and ignore any blank lines before the hunk content
                insideHunk = true;
                tempEmptyLines.Clear();
                currentHunk = new Hunk();
                hunks.Add(currentHunk);
            }
            else if (insideHunk)
            {
                // Remove empty lines between two hunks.
                // Removing empty lines can help prevent issues later in processing, especially in unified diffs.
                // Empty lines between hunks can create misleading offsets or gaps.
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    tempEmptyLines.Add(line);
                    continue;
                }

                // if empty lines are not in the end of hunk and should consider inside of hunk for processing.
                // keep empty lines inside of hunk.
                if (tempEmptyLines.Count != 0)
                {
                    // add previous empty lines
                    currentHunk?.HunkItems.AddRange(
                        tempEmptyLines.Select(x => new DiffHunkItem(ChangeType.Unchanged, x))
                    );
                    tempEmptyLines.Clear();
                }

                // Add the line to the current hunk with the appropriate change type
                currentHunk?.HunkItems.Add(
                    new DiffHunkItem(
                        trimmedLine.StartsWith("+") ? ChangeType.Add
                            : trimmedLine.StartsWith("-") ? ChangeType.Delete
                            : ChangeType.Unchanged,
                        line
                    )
                );
            }
        }

        return (hunks, originalPath, modifiedPath);
    }

    private static List<string> ExtractSearchLines(Hunk hunk)
    {
        return hunk
            .HunkItems.Where(item => item.ChangeType == ChangeType.Unchanged || item.ChangeType == ChangeType.Delete)
            .Select(item => item.Line.Substring(1))
            .ToList();
    }

    private static List<string> ExtractNewLines(Hunk hunk)
    {
        return hunk
            .HunkItems.Where(item => item.ChangeType == ChangeType.Add)
            .Select(item => item.Line.Substring(1))
            .ToList();
    }

    private static List<string> ExtractMarkdownCodeBlocks(string text)
    {
        const string backticks = "```";
        var codeBlocks = new List<string>();

        int startIdx = text.IndexOf(backticks, StringComparison.Ordinal);
        while (startIdx != -1)
        {
            int endIdx = text.IndexOf(backticks, startIdx + backticks.Length, StringComparison.Ordinal);
            if (endIdx != -1)
            {
                var codeBlock = text.Substring(startIdx + backticks.Length, endIdx - (startIdx + backticks.Length))
                    .Trim();
                codeBlocks.Add(codeBlock);
                startIdx = text.IndexOf(backticks, endIdx + backticks.Length, StringComparison.Ordinal);
            }
            else
            {
                break;
            }
        }

        return codeBlocks;
    }
}
