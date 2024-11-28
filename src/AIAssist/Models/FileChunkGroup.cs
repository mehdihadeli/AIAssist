using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssist.Models;

/// <summary>
/// Represents a file and its associated chunks for embedding.
/// </summary>
public class FileChunkGroup(CodeFileMap file, List<string> chunks)
{
    public CodeFileMap File { get; } = file;
    public IList<string> Chunks { get; } = chunks;

    public string Input => string.Join("\n", Chunks);
}
