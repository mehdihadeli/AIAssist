namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class ClassInfo
{
    public string Name { get; set; } = default!;
    public IList<MethodInfo> Methods { get; set; } = new List<MethodInfo>();
    public IList<FieldInfo> Fields { get; set; } = new List<FieldInfo>();
    public IList<PropertyInfo> Properties { get; set; } = new List<PropertyInfo>();
    public string Definition { get; set; } = default!;
}
