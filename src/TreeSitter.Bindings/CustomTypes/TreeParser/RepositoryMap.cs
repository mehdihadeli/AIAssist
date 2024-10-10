namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class RepositoryMap
{
    public IList<FileInfo> Files { get; set; } = new List<FileInfo>();

    // Add file information to the repository map
    public void AddFile(FileInfo fileInfo)
    {
        Files.Add(fileInfo);
    }
}
