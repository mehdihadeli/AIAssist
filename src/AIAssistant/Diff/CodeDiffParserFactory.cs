using AIAssistant.Contracts.Diff;
using AIAssistant.Models;

namespace AIAssistant.Diff;

public class CodeDiffParserFactory(IDictionary<CodeDiffType, ICodeDiffParser> strategies) : ICodeDiffParserFactory
{
    public ICodeDiffParser Create(CodeDiffType codeDiffType)
    {
        return strategies[codeDiffType];
    }
}
