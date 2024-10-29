namespace AIAssistant.Models;

public class CodeEmbedding
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SessionId { get; set; }
    public string Code { get; set; } = default!;
    public string TreeSitterFullCode { get; set; } = default!;
    public string TreeOriginalCode { get; set; } = default!;
    public IList<double> Embeddings { get; set; } = default!;
    public string RelativeFilePath { get; set; } = default!;
}
