namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class NamespaceInfo
{
    public string Name { get; set; } = default!;
    public IList<string> TopLevelStatements { get; set; } = new List<string>(); // Top-level statements (for C# 9)
    public IList<ClassInfo> Classes { get; set; } = new List<ClassInfo>();
    public IList<EnumInfo> Enums { get; set; } = new List<EnumInfo>();
    public IList<StructInfo> Structs { get; set; } = new List<StructInfo>();
    public IList<RecordInfo> Records { get; set; } = new List<RecordInfo>();
    public IList<InterfaceInfo> Interfaces { get; set; } = new List<InterfaceInfo>();
    public string Definition { get; set; } = default!;
}
