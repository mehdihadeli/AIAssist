namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class CaptureNode
{
    public required string CaptureKey { get; set; } = default!;
    public IList<TSNode> Values { get; set; } = new List<TSNode>();
}
