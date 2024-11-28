using AIAssist.Contracts.Diff;
using Clients.Models;

namespace AIAssist.Diff;

public class CodeDiffParserFactory(IDictionary<CodeDiffType, ICodeDiffParser> strategies) : ICodeDiffParserFactory
{
    public ICodeDiffParser Create(CodeDiffType codeDiffType)
    {
        return strategies[codeDiffType];
    }
}
