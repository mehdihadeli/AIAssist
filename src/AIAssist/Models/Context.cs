using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssist.Models;

public class Context
{
    public IList<BaseContextItem> ContextItems { get; } = new List<BaseContextItem>();
}

public class BaseContextItem
{
    public string Name { get; init; } = default!;
    public DateTime CreatedAt { get; } = DateTime.Now;
    public DateTime UpdatedAt { get; init; }
}

public class FolderItemContext : BaseContextItem
{
    public FolderItemContext(
        string path,
        string relativePath,
        IList<FolderItemContext> subFolders,
        IList<FileItemContext> files
    )
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(relativePath), "RelativePath cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(path), "Path cannot be null or empty.");
        }

        RelativePath = relativePath;
        Path = path;
        Name = System.IO.Path.GetFileName(relativePath);
        Files = files;
        SubFoldersItemContext = subFolders;
    }

    public string Path { get; }
    public string RelativePath { get; }
    public IList<FileItemContext> Files { get; }
    public IList<FolderItemContext> SubFoldersItemContext { get; }
}

public class FileItemContext : BaseContextItem
{
    public FileItemContext(string path, string relativePath, CodeFileMap codeFileMap)
    {
        RelativePath = relativePath;
        Path = path;
        Name = System.IO.Path.GetFileName(relativePath);
        CodeFileMap = codeFileMap;
    }

    public CodeFileMap CodeFileMap { get; }
    public string Path { get; }
    public string RelativePath { get; }
}

public class UrlItemContext : BaseContextItem
{
    public UrlItemContext(string url, string name)
    {
        Name = name;
        Url = url;
    }

    public string Url { get; set; }
}
