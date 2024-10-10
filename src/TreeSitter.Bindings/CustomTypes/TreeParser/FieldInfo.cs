namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class FieldInfo
{
    public string Name { get; set; } = default!;

    public string Type { get; set; } = default!;
    public string AccessModifier { get; set; } = default!;

    public string Accessibility { get; set; } = default!;

    public bool IsStatic { get; set; }

    public bool IsReadonly { get; set; }
    public string Definition { get; set; } = default!;
}
