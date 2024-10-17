namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class InterfaceInfo : ICodeElement, IMethodElement, IPropertyElement, ICommentElement
{
    public string Name { get; set; } = default!;
    public IList<MethodInfo> Methods { get; set; } = new List<MethodInfo>();
    public IList<PropertyInfo> Properties { get; set; } = new List<PropertyInfo>();
    public string Comments { get; set; } = default!;
    public string Definition { get; set; } = default!;
}
