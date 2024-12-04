namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class DefinitionCapture
{
    public IList<DefinitionCaptureItem> CaptureItems { get; init; } = new List<DefinitionCaptureItem>();
    public string RelativePath { get; init; } = default!;
    public string? Signiture { get; init; }
    public string CaptureGroup { get; init; } = default!;
    public string? Definition { get; init; }
    public string OriginalCode { get; init; } = default!;
}

public class DefinitionCaptureItem
{
    public string CaptureKey { get; set; } = default!;
    public string CaptureValue { get; set; } = default!;
    public int StartLine { get; set; }
    public int EndLine { get; set; }
};
