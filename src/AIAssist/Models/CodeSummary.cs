namespace AIAssist.Models;

public class CodeSummary : CodeBase
{
    public string TreeSitterSummarizeCode { get; set; } = default!;
    public bool UseFullCodeFile { get; set; }
}
