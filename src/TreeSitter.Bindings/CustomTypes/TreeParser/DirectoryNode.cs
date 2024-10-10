namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class DirectoryNode(string name)
{
    public string Name { get; set; } = name;
    public IDictionary<string, DirectoryNode> SubDirectories { get; set; } = new Dictionary<string, DirectoryNode>();
    public IList<FileInfo> Files { get; set; } = new List<FileInfo>();

    public DirectoryNode GetOrAddDirectory(string directoryName)
    {
        if (SubDirectories.TryGetValue(directoryName, out DirectoryNode? value))
            return value;
        value = new DirectoryNode(directoryName);
        SubDirectories[directoryName] = value;

        return value;
    }
}
