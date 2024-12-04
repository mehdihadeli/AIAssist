namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class CodeFileMap
{
    public string TreeSitterSummarizeCode { get; set; } = default!;
    public string TreeSitterFullCode { get; set; } = default!;
    public string OriginalCode { get; set; } = default!;
    public string TreeOriginalCode { get; set; } = default!;
    public string RelativePath { get; set; } = default!;
    public string Path { get; set; } = default!;
}
