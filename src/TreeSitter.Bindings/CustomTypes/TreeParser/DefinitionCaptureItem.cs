namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class DefinitionCaptureItem
{
    public string CaptureKey { get; set; } = default!;
    public string CaptureValue { get; set; } = default!;
    public IList<DefinitionCaptureReference> DefinitionCaptureReferences { get; set; } =
        new List<DefinitionCaptureReference>();
    public string RelativePath { get; set; } = default!;
    public string CodeChunk { get; set; } = default!;
    public string OriginalCode { get; set; } = default!;
    public CaptureType CaptureType { get; set; }
}

public class DefinitionCaptureReference
{
    public string FileName { get; set; } = default!;
    public string Value { get; set; } = default!;
    public string Key { get; set; } = default!;
}
