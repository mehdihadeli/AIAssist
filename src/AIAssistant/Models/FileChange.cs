namespace AIAssistant.Models;

public class FileChange(string filePath, bool isNewFile = false, bool isDeletedFile = false)
{
    public string FilePath { get; set; } = filePath;
    public bool IsNewFile { get; set; } = isNewFile;
    public bool IsDeletedFile { get; set; } = isDeletedFile;
    public IList<FileChangeLine> ChangeLines { get; set; } = new List<FileChangeLine>();

    // Constructor for full content updates (like FileSnipped diff)
    public FileChange(string filePath, string fileContent)
        : this(filePath, true, false)
    {
        ChangeLines = fileContent
            .Split('\n')
            .Select((line, index) => new FileChangeLine(index + 1, line, true))
            .ToList();
    }
}

public class FileChangeLine(int lineNumber, string content, bool isAddition)
{
    public int LineNumber { get; set; } = lineNumber;
    public string Content { get; set; } = content;
    public bool IsAddition { get; set; } = isAddition;
}
