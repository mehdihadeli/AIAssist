namespace AIAssist.Models;

public class Replacement(int originalFileStartIndex, int originalFileEndIndex, List<string>? newLines)
{
    /// <summary>
    /// Start position of original file to replace.
    /// </summary>
    public int OriginalFileStartIndex { get; } = originalFileStartIndex;

    /// <summary>
    /// End position of original file to replace.
    /// </summary>
    public int OriginalFileEndIndex { get; } = originalFileEndIndex;

    /// <summary>
    /// New lines to replace.
    /// </summary>
    public IList<string> NewLines { get; } = newLines ?? new List<string>();
}
