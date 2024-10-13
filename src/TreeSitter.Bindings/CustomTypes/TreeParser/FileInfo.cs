namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class FileInfo
{
    public string Path { get; set; } = default!;

    /// <summary>
    /// top level statements outside a specific namespace
    /// </summary>
    public IList<string> TopLevelStatementsDefinition { get; set; } = new List<string>();
    public IList<NamespaceInfo> Namespaces { get; } = new List<NamespaceInfo>();

    /// <summary>
    /// Top level classes outside a specific namespace
    /// </summary>
    public IList<ClassInfo> Classes { get; } = new List<ClassInfo>();

    /// <summary>
    /// Top level structs outside a specific namespace
    /// </summary>
    public IList<StructInfo> Structs { get; } = new List<StructInfo>();

    /// <summary>
    /// Top level enums outside a specific namespace
    /// </summary>
    public IList<EnumInfo> Enums { get; } = new List<EnumInfo>();

    /// <summary>
    /// Top level interfaces outside a specific namespace
    /// </summary>
    public IList<InterfaceInfo> Interfaces { get; } = new List<InterfaceInfo>();

    /// <summary>
    /// Top level records outside a specific namespace
    /// </summary>
    public IList<RecordInfo> Records { get; } = new List<RecordInfo>();
}
