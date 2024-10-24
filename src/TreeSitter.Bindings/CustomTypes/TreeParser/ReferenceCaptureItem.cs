namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class ReferenceCaptureItem
{
    public string CaptureKey { get; set; } = default!;
    public string CaptureValue { get; set; } = default!;
    public string RelativePath { get; set; } = default!;
    public string? CodeChunk { get; set; }
    public string? Definition { get; set; }
    public string OriginalCode { get; set; } = default!;
}
