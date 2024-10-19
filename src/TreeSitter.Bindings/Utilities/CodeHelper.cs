namespace TreeSitter.Bindings.Utilities;

public static class CodeHelper
{
    public static IEnumerable<string> GetLinesOfInterest(
        string codeBlock,
        int[] lineNumbers,
        string[]? endPatterns = null,
        string[]? stopPatterns = null
    )
    {
        // Split the code block into lines
        string[] lines = codeBlock.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
        List<string> linesOfInterest = new List<string>();

        // Iterate over the line numbers and add the corresponding lines to the result
        foreach (int lineNumber in lineNumbers)
        {
            if (lineNumber > 0 && lineNumber <= lines.Length)
            {
                int currentLineIndex = lineNumber - 1; // Convert to 0-based index
                string currentLine = lines[currentLineIndex];

                // Check for stop patterns first
                if (
                    stopPatterns is { Length: > 0 }
                    && stopPatterns.Any(pattern => currentLine.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                )
                {
                    return linesOfInterest;
                }

                linesOfInterest.Add(currentLine);

                List<string> patternList = new List<string>();
                // If endPatterns are provided, continue adding lines until one ends with any of the patterns
                if (endPatterns is { Length: > 0 })
                {
                    while (currentLineIndex + 1 < lines.Length)
                    {
                        currentLineIndex++;
                        currentLine = lines[currentLineIndex];

                        // Check for stop patterns during iteration
                        if (
                            stopPatterns is { Length: > 0 }
                            && stopPatterns.Any(pattern =>
                                currentLine.Contains(pattern, StringComparison.OrdinalIgnoreCase)
                            )
                        )
                        {
                            // Stop processing further and return the initial line of interest
                            return linesOfInterest;
                        }

                        patternList.Add(currentLine);

                        if (endPatterns.Any(pattern => currentLine.TrimEnd().EndsWith(pattern)))
                        {
                            linesOfInterest.AddRange(patternList);
                            break;
                        }
                    }
                }
            }
            else
            {
                linesOfInterest.Add(string.Empty); // Add empty line if out of range
            }
        }

        return linesOfInterest;
    }

    public static string GetLinesOfInterest(
        string codeBlock,
        int lineNumber,
        string[]? endPatterns = null,
        string[]? stopPatterns = null
    )
    {
        return string.Join(Environment.NewLine, GetLinesOfInterest(codeBlock, [lineNumber], endPatterns, stopPatterns));
    }

    public static string GetChunkOfLines(string codeBlock, int startLineNumber, int endLineNumber)
    {
        // Split the code block into lines
        string?[] lines = codeBlock.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

        // Ensure the start and end line numbers are within a valid range
        startLineNumber = Math.Max(1, startLineNumber); // Line numbers are 1-based
        endLineNumber = Math.Min(lines.Length, endLineNumber);

        // Get the lines within the specified range
        var linesOfInterest = new List<string?>();
        for (int i = startLineNumber; i <= endLineNumber; i++)
        {
            linesOfInterest.Add(lines[i - 1]); // Line numbers are 1-based
        }

        return string.Join(Environment.NewLine, linesOfInterest);
    }
}
