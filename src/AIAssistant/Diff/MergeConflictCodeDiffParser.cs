using System.Text.RegularExpressions;
using AIAssistant.Contracts.Diff;
using AIAssistant.Models;

namespace AIAssistant.Diff;

//
// public class MergeConflictCodeDiffParser : ICodeDiffParser
// {
//     // private static readonly Regex _filePathRegex = new(@"^([\w\-./\\]+?\.[\w]+)$", RegexOptions.Compiled);
//     // private static readonly Regex _previousVersionStart = new(@"^<<<<<<< PREVIOUS VERSION$", RegexOptions.Compiled);
//     // private static readonly Regex _newVersionEnd = new(@"^>>>>>>> NEW VERSION$", RegexOptions.Compiled);
//     // private static readonly Regex _separator = new(@"^=======$", RegexOptions.Compiled);
//     //
//     // public IList<FileChange> GetFileChanges(string diff)
//     // {
//     //     var changes = new List<FileChange>();
//     //     string? currentFilePath = null;
//     //     var hunks = new List<List<FileChangeLine>>();
//     //     List<FileChangeLine>? currentHunk = null;
//     //
//     //     bool isPreviousVersion = false;
//     //     bool isNewVersion = false;
//     //
//     //     var lines = diff.Split('\n');
//     //
//     //     foreach (var line in lines)
//     //     {
//     //         // Detect a new file path, starting a new `MergeConflict` section
//     //         if (_filePathRegex.IsMatch(line.Trim()))
//     //         {
//     //             // Finalize the previous file change if there are accumulated hunks
//     //             if (currentFilePath != null && hunks.Count > 0)
//     //             {
//     //                 var fileChangeLines = hunks.SelectMany(h => h).ToList();
//     //                 var fileChangeType = DetermineFileChangeType(fileChangeLines);
//     //                 changes.Add(new FileChange(currentFilePath, fileChangeType, fileChangeLines));
//     //                 hunks.Clear();
//     //             }
//     //
//     //             currentFilePath = line.Trim();
//     //             continue;
//     //         }
//     //
//     //         // Start of a new `PREVIOUS VERSION/NEW VERSION` hunk
//     //         if (_previousVersionStart.IsMatch(line.Trim()))
//     //         {
//     //             isPreviousVersion = true;
//     //             isNewVersion = false;
//     //             currentHunk = new List<FileChangeLine>();
//     //             continue;
//     //         }
//     //
//     //         // Separator between previous and new version
//     //         if (_separator.IsMatch(line.Trim()))
//     //         {
//     //             isPreviousVersion = false;
//     //             isNewVersion = true;
//     //             continue;
//     //         }
//     //
//     //         // End of the hunk's new version
//     //         if (_newVersionEnd.IsMatch(line.Trim()))
//     //         {
//     //             isNewVersion = false;
//     //             if (currentHunk != null)
//     //             {
//     //                 hunks.Add(currentHunk);
//     //                 currentHunk = null;
//     //             }
//     //             continue;
//     //         }
//     //
//     //         // Collect lines within each `PREVIOUS VERSION` or `NEW VERSION` as part of the current hunk
//     //         if (isPreviousVersion && currentHunk != null)
//     //         {
//     //             currentHunk.Add(new FileChangeLine(0, line, CodeChangeType.Delete)); // 0 here because we're not tracking line numbers yet
//     //         }
//     //         else if (isNewVersion && currentHunk != null)
//     //         {
//     //             currentHunk.Add(new FileChangeLine(0, line, CodeChangeType.Add)); // 0 here because we're not tracking line numbers yet
//     //         }
//     //     }
//     //
//     //     // Finalize the last file change if any hunks remain
//     //     if (currentFilePath != null && hunks.Count > 0)
//     //     {
//     //         var fileChangeLines = hunks.SelectMany(h => h).ToList();
//     //         var fileChangeType = DetermineFileChangeType(fileChangeLines);
//     //         changes.Add(new FileChange(currentFilePath, fileChangeType, fileChangeLines));
//     //     }
//     //
//     //     return changes;
//     // }
//     //
//     // private CodeChangeType DetermineFileChangeType(IList<FileChangeLine> changeLines)
//     // {
//     //     bool allAdded = changeLines.All(line => line.LineCodeChangeType == CodeChangeType.Add);
//     //     bool allDeleted = changeLines.All(line => line.LineCodeChangeType == CodeChangeType.Delete);
//     //
//     //     if (allAdded)
//     //         return CodeChangeType.Add; // Newly created file
//     //     if (allDeleted)
//     //         return CodeChangeType.Delete; // Deleted file
//     //
//     //     return CodeChangeType.Update; // Modified existing file
//     // }
// }
