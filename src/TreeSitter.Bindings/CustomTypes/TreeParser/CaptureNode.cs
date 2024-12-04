namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class CaptureNode
{
    public required string CaptureKey { get; set; } = default!;
    public required string CaptureGroup { get; set; } = default!;
    public TSNode Value { get; set; }
}
