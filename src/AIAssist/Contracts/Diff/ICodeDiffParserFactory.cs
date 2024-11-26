using Clients.Models;

namespace AIAssist.Contracts.Diff;

public interface ICodeDiffParserFactory
{
    ICodeDiffParser Create(CodeDiffType codeDiffType);
}
