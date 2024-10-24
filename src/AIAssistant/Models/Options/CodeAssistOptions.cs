namespace AIAssistant.Models.Options;

public class CodeAssistOptions
{
    public string ContextWorkingDirectory { get; set; } = default!;
    public bool AutoContextEnabled { get; set; } = true;
    public DiffType DiffType { get; set; } = DiffType.FileSnippedDiff;
    public CodeAssistStrategyType CodeAssistType { get; set; } = CodeAssistStrategyType.Embedding;
    public IEnumerable<string> Files { get; set; } = new List<string>();
}
