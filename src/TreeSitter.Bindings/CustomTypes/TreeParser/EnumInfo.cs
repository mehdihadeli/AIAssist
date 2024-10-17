namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class EnumInfo : ICodeElement, ICommentElement
{
    public string Name { get; set; } = default!;
    public string Comments { get; set; } = default!;
    public IList<string> Members { get; set; } = new List<string>();
    public string Definition { get; set; } = default!;
}
