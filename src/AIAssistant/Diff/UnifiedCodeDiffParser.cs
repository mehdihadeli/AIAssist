using System.Diagnostics;
using System.Text.RegularExpressions;
using AIAssistant.Contracts.Diff;
using AIAssistant.Models;

namespace AIAssistant.Diff;

public class UnifiedCodeDiffParser : ICodeDiffParser
{
    private static readonly Regex _diffHeaderRegex = new(@"@@ -(\d+),?(\d+)? \+(\d+),?(\d+)? @@");

    public bool GitPatch { get; set; }

    public IList<FileChange> GetFileChanges(string diff)
    {
        if (GitPatch)
        {
            return ApplyUsingGitPatch(diff);
        }

        return ParseUnifiedDiff(diff);
    }

    private IList<FileChange> ParseUnifiedDiff(string diff)
    {
        var fileChanges = new List<FileChange>();
        FileChange? currentFileChange = null;

        var lines = diff.Split('\n');
        int currentLineNumberOriginal = 0;
        int currentLineNumberNew = 0;
        bool insideCodeBlock = false;
        bool isFileCreation = false;

        foreach (var lineItem in lines)
        {
            var line = lineItem.Trim();

            // Detect start and end of the code block
            if (line.StartsWith("```diff"))
            {
                insideCodeBlock = true;
                continue;
            }
            else if (line.StartsWith("```"))
            {
                insideCodeBlock = false;
                continue;
            }

            // If not inside a code block, ignore the line
            if (!insideCodeBlock)
                continue;

            // Match the file header
            if (line.StartsWith("--- "))
            {
                // If there's an ongoing file change, add it to the list
                if (currentFileChange != null)
                {
                    fileChanges.Add(currentFileChange);
                }

                var filePath = line.Substring(4).Trim();
                if (filePath == "/dev/null")
                {
                    isFileCreation = true;
                    continue; // Handle file creation in the "+++" section
                }
                else
                {
                    currentFileChange = new FileChange(filePath, CodeChangeType.Update);
                    isFileCreation = false;
                }
            }
            else if (line.StartsWith("+++ "))
            {
                var filePath = line.Substring(4).Trim();
                if (filePath == "/dev/null")
                {
                    // File deletion
                    currentFileChange.FileCodeChangeType = CodeChangeType.Delete;
                }
                else if (isFileCreation)
                {
                    // File creation
                    currentFileChange = new FileChange(filePath, CodeChangeType.Add);
                    isFileCreation = false;
                }
                else if (currentFileChange?.FilePath != filePath)
                {
                    // File move/rename: treat as delete + add
                    if (currentFileChange != null)
                    {
                        fileChanges.Add(new FileChange(currentFileChange.FilePath, CodeChangeType.Delete));
                    }
                    currentFileChange = new FileChange(filePath, CodeChangeType.Add);
                }
                continue;
            }
            // Match the diff headers with line numbers
            else if (line.StartsWith("@@"))
            {
                var match = _diffHeaderRegex.Match(line);
                if (match.Success)
                {
                    currentLineNumberOriginal = int.Parse(match.Groups[1].Value);
                    currentLineNumberNew = int.Parse(match.Groups[3].Value);
                }
            }
            // Handle removed lines
            else if (line.StartsWith("-"))
            {
                currentFileChange?.ChangeLines.Add(
                    new FileChangeLine(currentLineNumberOriginal++, line.Substring(1).Trim(), CodeChangeType.Delete)
                );
            }
            // Handle added lines
            else if (line.StartsWith("+"))
            {
                currentFileChange?.ChangeLines.Add(
                    new FileChangeLine(currentLineNumberNew++, line.Substring(1).Trim(), CodeChangeType.Add)
                );
            }
            else
            {
                // Handle unchanged lines, which are usually part of the context
                currentFileChange?.ChangeLines.Add(
                    new FileChangeLine(currentLineNumberOriginal++, line, CodeChangeType.Update)
                );
                // Increment both line counters as line is unchanged
                currentLineNumberNew++;
            }
        }

        if (currentFileChange != null)
        {
            fileChanges.Add(currentFileChange);
        }

        return fileChanges;
    }

    private IList<FileChange> ApplyUsingGitPatch(string diff)
    {
        // we don't add any file to collection because changes will apply automatically with git apply
        var fileChanges = new List<FileChange>();

        // Write the diff to a temporary file
        var tempFilePath = Path.GetTempFileName();
        File.WriteAllText(tempFilePath, diff);

        try
        {
            // Use git apply command to apply the diff
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"apply \"{tempFilePath}\"",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                // Successfully applied the patch
            }
            else
            {
                // Failed to apply the patch, log or handle errors
                string errorOutput = process.StandardError.ReadToEnd();
                throw new Exception($"Git apply failed: {errorOutput}");
            }
        }
        finally
        {
            // Delete the temporary file
            File.Delete(tempFilePath);
        }

        return fileChanges;
    }
}
