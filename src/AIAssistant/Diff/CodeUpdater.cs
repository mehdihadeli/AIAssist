namespace AIAssistant.Diff;

public class CodeUpdater
{
    public void ApplyChanges(IEnumerable<Change> changes)
    {
        foreach (var change in changes)
        {
            var filePath = change.FilePath;

            // Handle file creation if it's a new file
            if (change.IsNewFile)
            {
                var newFileLines = change.Changes.Where(c => c.IsAddition).Select(c => c.Content).ToList();

                File.WriteAllLines(filePath, newFileLines);
                continue;
            }

            // If the file is marked for deletion
            if (change.IsDeletedFile)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                continue;
            }

            // Read the original file content if it exists
            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath).ToList();

                // Process changes in reverse order for proper line indexing
                foreach (var lineChange in change.Changes.OrderByDescending(c => c.LineNumber))
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
                File.WriteAllLines(filePath, lines);
            }
        }
    }
}
