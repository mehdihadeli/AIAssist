namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class PropertyInfo : ICodeElement
{
    public string Name { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string AccessModifier { get; set; } = default!;
    public string Accessibility { get; set; } = default!;
    public bool IsStatic { get; set; } = default!;
    public bool IsReadOnly { get; set; } = default!;
    public string Comments { get; set; } = default!;
    public string Definition { get; set; } = default!;
}
