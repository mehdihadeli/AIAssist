namespace AIRefactorAssistant.Models;

public class CodeEmbedding
{
    public string RelativeFilePath { get; set; }
    public string Chunk { get; set; }
    public string Embedding { get; set; }
}
