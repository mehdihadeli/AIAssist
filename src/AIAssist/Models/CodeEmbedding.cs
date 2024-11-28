namespace AIAssist.Models;

public class CodeEmbedding : CodeSummary
{
    public string TreeSitterFullCode { get; set; } = default!;
    public IList<double> Embeddings { get; set; } = default!;
}
