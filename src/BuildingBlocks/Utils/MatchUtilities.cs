namespace BuildingBlocks.Utils;

public static class MatchUtilities
{
    public static int FindMatchIndex(IList<string> origLines, IList<string> newLines)
    {
        // Create copies of the lists so the originals aren't modified.
        var origLinesCopy = new List<string>(origLines);
        var newLinesCopy = new List<string>(newLines);

        int index = ExactMatch(origLinesCopy, newLinesCopy);

        if (index == -1)
        {
            // Remove whitespace from the beginning and end of each line
            origLinesCopy = origLinesCopy.Select(s => s.Trim()).ToList();
            newLinesCopy = newLinesCopy.Select(s => s.Trim()).ToList();
            index = ExactMatch(origLinesCopy, newLinesCopy);

            if (index == -1)
            {
                // Filter out any empty strings
                var newOrigLines = origLinesCopy.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                var newNewLines = newLinesCopy.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

                index = ExactMatch(newOrigLines, newNewLines);

                // If there was a match in the filtered lists, map it back to the original list
                if (newOrigLines.Count != 0 && index != -1)
                {
                    index = origLinesCopy.IndexOf(newOrigLines[index]);
                }
            }
        }

        return index;
    }

    private static int ExactMatch(List<string> originalLines, List<string> newLines)
    {
        // Check for both lists being empty or containing only whitespace
        if (string.Join("", newLines).Trim() == "" && string.Join("", originalLines).Trim() == "")
        {
            return 0;
        }

        for (var i = 0; i <= originalLines.Count - newLines.Count; i++)
        {
            if (originalLines.GetRange(i, newLines.Count).SequenceEqual(newLines))
            {
                return i;
            }
        }

        return -1;
    }
}
