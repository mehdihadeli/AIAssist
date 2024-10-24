namespace AIAssistant.Diff.CodeBlock;

public class CodeBlock(string filePath, string fileContent)
{
    public string FilePath { get; set; } = filePath;
    public string FileContent { get; set; } = fileContent;
}
