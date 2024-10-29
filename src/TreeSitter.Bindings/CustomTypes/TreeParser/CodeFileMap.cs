namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class CodeFileMap
{
    public string TreeSitterSummarizeCode { get; set; } = default!;
    public string TreeSitterFullCode { get; set; } = default!;
    public string OriginalCode { get; set; } = default!;
    public string TreeOriginalCode { get; set; } = default!;
    public string RelativePath { get; set; } = default!;
    public IEnumerable<ReferencedCodeMap> ReferencedCodesMap { get; set; } = default!;
}

public class ReferencedCodeMap
{
    public string RelativePath { get; set; } = default!;
    public string ReferencedValue { get; set; } = default!;
    public string ReferencedUsage { get; set; } = default!;
}
