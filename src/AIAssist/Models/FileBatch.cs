namespace AIAssist.Models;

/// <summary>
/// Represents a batch of files and their chunks to be processed in a single embedding request.
/// </summary>
public class FileBatch
{
    public IList<FileChunkGroup> Files { get; set; } = new List<FileChunkGroup>();
    public int TotalTokens { get; set; }

    /// <summary>
    /// Combines all chunked inputs for this batch into a single list for API calls.
    /// </summary>
    public IList<string> GetBatchInputs()
    {
        return Files.SelectMany(file => file.Chunks).ToList();
    }
}
