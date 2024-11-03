namespace AIAssistant.Models.Options;

public class CodeAssistOptions
{
    public string ContextWorkingDirectory { get; set; } = default!;
    public bool AutoContextEnabled { get; set; } = true;
    public CodeDiffType CodeDiffType { get; set; } = CodeDiffType.MergeConflictDiff;
    public CodeAssistType CodeAssistType { get; set; } = CodeAssistType.Embedding;
    public IList<string>? Files { get; set; }
}
