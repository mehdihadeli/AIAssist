using AIAssistant.Models;

namespace AIAssistant.Contracts.Diff;

public interface ICodeDiffParserFactory
{
    ICodeDiffParser Create(CodeDiffType codeDiffType);
}
