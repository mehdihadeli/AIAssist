namespace AIAssistant.Models;

public class FileChange
{
    public string FilePath { get; set; }
    public CodeChangeType FileCodeChangeType { get; set; }
    public IList<FileChangeLine> ChangeLines { get; set; }

    public FileChange(string filePath, CodeChangeType fileCodeChangeType, IList<FileChangeLine>? changeLines = null)
    {
        FilePath = filePath;
        FileCodeChangeType = fileCodeChangeType;
        ChangeLines = changeLines ?? new List<FileChangeLine>();
    }

    // Constructor for new or deleted files
    public FileChange(string filePath, string fileContent, CodeChangeType codeChangeType)
    {
        FilePath = filePath;
        FileCodeChangeType = codeChangeType;

        ChangeLines = fileContent
            .Split('\n')
            .Select((line, index) => new FileChangeLine(index + 1, line, codeChangeType))
            .ToList();
    }
}

public class FileChangeLine(int lineNumber, string content, CodeChangeType lineCodeChangeType)
{
    public int LineNumber { get; set; } = lineNumber;
    public string Content { get; set; } = content;
    public CodeChangeType LineCodeChangeType { get; set; } = lineCodeChangeType;
}

public enum CodeChangeType
{
    Add,
    Update,
    Delete,
}
