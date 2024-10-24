namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class DefinitionCaptureItem
{
    public string CaptureKey { get; set; } = default!;
    public string CaptureValue { get; set; } = default!;
    public IList<DefinitionCaptureReference> DefinitionCaptureReferences { get; } =
        new List<DefinitionCaptureReference>();
    public string RelativePath { get; set; } = default!;
    public string? CodeChunk { get; set; }
    public string? Definition { get; set; }
    public string OriginalCode { get; set; } = default!;
}

public class DefinitionCaptureReference
{
    public string RelativePath { get; set; } = default!;
    public string ReferencedValue { get; set; } = default!;
    public string ReferencedUsage { get; set; } = default!;
}
