using System.Text.RegularExpressions;
using AIAssist.Contracts.Diff;
using AIAssist.Models;

namespace AIAssist.Diff;

// https://en.wikipedia.org/wiki/Diff#Unified_format


public class UnifiedDiffParser : ICodeDiffParser
{
    private readonly string _originalFileHunkMarker = "---";
    private readonly string _modifiedFileHunkMarker = "+++";
    private readonly Regex _hunkHeaderRegex = new(@"^@@\s*-(\d+),?(\d*)\s*\+(\d+),?(\d*)\s*@@");

    public IList<DiffResult> ParseDiffResults(string diffContent, string contextWorkingDirectory)
    {
        // Extract all code blocks enclosed in backticks
        var codeBlocks = ExtractMarkdownCodeBlocks(diffContent);
        var diffResults = new List<DiffResult>();

        foreach (var codeBlock in codeBlocks)
        {
            var (hunks, originalPath, modifiedPath) = ExtractHunks(codeBlock);

            var replacements = ProcessHunks(hunks, originalPath, modifiedPath, contextWorkingDirectory);

            var action = DetermineAction(originalPath, modifiedPath);

            diffResults.Add(
                new DiffResult
                {
                    OriginalPath = originalPath,
                    ModifiedPath = modifiedPath,
                    Action = action,
                    Replacements = replacements,
                    ModifiedLines = null,
                }
            );
        }

        return diffResults;
    }

    private static ActionType DetermineAction(string originalPath, string modifiedPath)
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
        var replacements = new List<Replacement>();

        const string devNull = "/dev/null";

        // Handle file addition
        if (originalPath == devNull)
        {
            foreach (var hunk in hunks)
            {
                // For additions, all lines are treated as added
                replacements.AddRange(BuildReplacements(hunk, 0));
            }

            return replacements;
        }

        // Handle file deletion
        if (modifiedPath == devNull)
        {
            foreach (var hunk in hunks)
            {
                // For deletions, all lines are treated as deleted starting from the hunk's OriginalStart
                replacements.AddRange(BuildReplacements(hunk, hunk.OriginalStart - 1));
            }

            return replacements;
        }

        // handle file modification
        var filePath = Path.Combine(contextWorkingDirectory, originalPath);
        var originalLines = File.Exists(filePath) ? File.ReadAllLines(filePath).ToList() : new List<string>();

        foreach (var hunk in hunks)
        {
            if (hunk.HunkItems.Count == 0)
                continue;

            // Convert to 0-based index, because hunk header lines start from 1
            int startLine = hunk.OriginalStart - 1;

            if (startLine < 0 || startLine >= originalLines.Count)
                continue;

            AlignEmptyLines(hunk, originalLines, startLine);
            replacements.AddRange(BuildReplacements(hunk, startLine));
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

        string originalPath = string.Empty;
        string modifiedPath = string.Empty;

        foreach (var line in lines)
        {
            if (line.StartsWith(_originalFileHunkMarker))
            {
                originalPath = line.Substring(_originalFileHunkMarker.Length).Trim();
            }
            else if (line.StartsWith(_modifiedFileHunkMarker))
            {
                modifiedPath = line.Substring(_modifiedFileHunkMarker.Length).Trim();
            }
            else if (_hunkHeaderRegex.IsMatch(line))
            {
                currentHunk = ParseHunkHeader(line);
                hunks.Add(currentHunk);
            }
            else if (currentHunk != null)
            {
                var changeType =
                    line.StartsWith("+") ? ChangeType.Add
                    : line.StartsWith("-") ? ChangeType.Delete
                    : ChangeType.Unchanged;

                currentHunk.HunkItems.Add(new DiffHunkItem(changeType, line));
            }
        }

        return (hunks, originalPath, modifiedPath);
    }

    private Hunk ParseHunkHeader(string headerLine)
    {
        var match = _hunkHeaderRegex.Match(headerLine);

        if (!match.Success)
            throw new InvalidOperationException($"Invalid hunk header: {headerLine}");

        return new Hunk
        {
            OriginalStart = int.Parse(match.Groups[1].Value),
            OriginalCount =
                match.Groups[2].Success && !string.IsNullOrWhiteSpace(match.Groups[2].Value)
                    ? int.Parse(match.Groups[2].Value)
                    : 0,
            ModifiedStart = int.Parse(match.Groups[3].Value),
            ModifiedCount =
                match.Groups[4].Success && !string.IsNullOrWhiteSpace(match.Groups[4].Value)
                    ? int.Parse(match.Groups[4].Value)
                    : 0,
            HunkItems = new List<DiffHunkItem>(),
        };
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
