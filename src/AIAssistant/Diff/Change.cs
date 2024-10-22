namespace AIAssistant.Diff;

public class Change(string filePath)
{
    public string FilePath { get; set; } = filePath;
    public bool IsNewFile { get; set; }
    public bool IsDeletedFile { get; set; }
    public IList<ChangeLine> Changes { get; set; } = new List<ChangeLine>();
}

public class ChangeLine(int lineNumber, string content, bool isAddition)
{
    public int LineNumber { get; set; } = lineNumber;
    public string Content { get; set; } = content;
    public bool IsAddition { get; set; } = isAddition;
}
