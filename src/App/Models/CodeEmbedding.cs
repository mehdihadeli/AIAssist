namespace AIRefactorAssistant.Models;

public class CodeEmbedding
{
    public string ClassName { get; set; }
    public string MethodsName { get; set; }
    public string Code { get; set; }
    public string EmbeddingData { get; set; } // Generated embeddings (a comma-separated vector string)
    public string RelativeFilePath { get; set; } // File path for chunk reference
}
