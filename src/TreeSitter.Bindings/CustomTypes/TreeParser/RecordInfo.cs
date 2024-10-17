namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class RecordInfo : ICodeElement, IMethodElement, IFieldElement, IPropertyElement, ICommentElement
{
    public string Name { get; set; } = default!;
    public IList<MethodInfo> Methods { get; set; } = new List<MethodInfo>();
    public IList<FieldInfo> Fields { get; set; } = new List<FieldInfo>();
    public IList<PropertyInfo> Properties { get; set; } = new List<PropertyInfo>();
    public string Comments { get; set; } = default!;
    public string Definition { get; set; } = default!;
}
