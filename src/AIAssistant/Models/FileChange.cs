namespace AIAssistant.Models;

public class FileChange
{
    public string FilePath { get; set; }
    public ChangeType FileChangeType { get; set; }
    public IList<FileChangeLine> ChangeLines { get; set; }

    public FileChange(string filePath, ChangeType fileChangeType, IList<FileChangeLine>? changeLines = null)
    {
        FilePath = filePath;
        FileChangeType = fileChangeType;
        ChangeLines = changeLines ?? new List<FileChangeLine>();
    }

    // Constructor for new or deleted files
    public FileChange(string filePath, string fileContent, ChangeType changeType)
    {
        FilePath = filePath;
        FileChangeType = changeType;

        ChangeLines = fileContent
            .Split('\n')
            .Select((line, index) => new FileChangeLine(index + 1, line, changeType))
            .ToList();
    }
}

public class FileChangeLine(int lineNumber, string content, ChangeType lineChangeType)
{
    public int LineNumber { get; set; } = lineNumber;
    public string Content { get; set; } = content;
    public ChangeType LineChangeType { get; set; } = lineChangeType;
}

public enum ChangeType
{
    Add,
    Update,
    Delete,
}
