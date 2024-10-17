namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class MethodInfo : ICodeElement, ICommentElement
{
    public string Name { get; set; } = default!;
    public string ReturnType { get; set; } = default!;
    public string AccessModifier { get; set; } = default!;
    public IList<ParameterInfo> Parameters { get; set; } = new List<ParameterInfo>();
    public string Comments { get; set; } = default!;
    public string Definition { get; set; } = default!;
}

public class ParameterInfo
{
    public string Name { get; set; } = default!;
    public string Type { get; set; } = default!;
}
