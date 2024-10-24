using System.Text.RegularExpressions;

namespace AIAssistant.Diff;

public class DiffParser
{
    private static readonly Regex _diffHeaderRegex = new Regex(@"@@ -(\d+),?(\d+)? \+(\d+),?(\d+)? @@");

    public IEnumerable<Change> ParseUnifiedDiff(string diff)
    {
        var changes = new List<Change>();
        Change currentChange = null;

        var lines = diff.Split('\n');
        int currentLineNumberOriginal = 0;
        int currentLineNumberNew = 0;

        bool insideCodeBlock = false;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

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
                if (currentChange != null)
                {
                    changes.Add(currentChange);
                }
                var filePath = line.Substring(4).Trim();

                if (filePath == "/dev/null")
                {
                    currentChange = new Change(lines[i + 1].Substring(4).Trim()) { IsNewFile = true };
                }
                else
                {
                    currentChange = new Change(filePath);
                }
            }
            else if (line.StartsWith("+++ "))
            {
                continue; // Skip the "+++" line as it's handled in the previous step
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
                currentChange?.Changes.Add(
                    new ChangeLine(currentLineNumberOriginal++, line.Substring(1).Trim(), false)
                );
            }
            // Handle added lines
            else if (line.StartsWith("+"))
            {
                currentChange?.Changes.Add(new ChangeLine(currentLineNumberNew++, line.Substring(1).Trim(), true));
            }
        }

        if (currentChange != null)
        {
            changes.Add(currentChange);
        }

        return changes;
    }
}
