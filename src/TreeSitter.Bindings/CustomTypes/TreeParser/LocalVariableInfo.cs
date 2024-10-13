namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class LocalVariableInfo : ICodeElement
{
    public string Name { get; set; } = default!;
    public string Definition { get; set; } = default!;
    public string Type { get; set; } = default!;
}
