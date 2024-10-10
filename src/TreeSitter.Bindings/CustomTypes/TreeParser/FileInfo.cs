namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class FileInfo
{
    public string Path { get; set; } = default!;
    public IList<NamespaceInfo> Namespaces { get; } = new List<NamespaceInfo>();
    public IList<ClassInfo> Classes { get; } = new List<ClassInfo>();
    public IList<StructInfo> Structs { get; } = new List<StructInfo>();
    public IList<EnumInfo> Enums { get; } = new List<EnumInfo>();
    public IList<InterfaceInfo> Interfaces { get; } = new List<InterfaceInfo>();
    public IList<RecordInfo> Records { get; } = new List<RecordInfo>();
}
