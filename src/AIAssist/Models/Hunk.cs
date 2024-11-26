namespace AIAssist.Models;

public class Hunk
{
    public int OriginalStart { get; set; }
    public int OriginalCount { get; set; }
    public int ModifiedStart { get; set; }
    public int ModifiedCount { get; set; }
    public List<DiffHunkItem> HunkItems { get; set; } = new();
}

public record DiffHunkItem(ChangeType ChangeType, string Line);
