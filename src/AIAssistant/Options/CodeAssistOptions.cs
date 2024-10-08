using AIAssistant.Models;

namespace AIAssistant.Options;

public class CodeAssistOptions
{
    public string ContextWorkingDirectory { get; set; } = default!;
    public bool AutoContextEnabled { get; set; } = true;
    public DiffType DiffType { get; set; } = DiffType.GitDiff;
    public IEnumerable<string> Files { get; set; } = new List<string>();
}
