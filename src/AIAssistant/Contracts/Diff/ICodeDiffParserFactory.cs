using AIAssistant.Models;
using Clients.Models;

namespace AIAssistant.Contracts.Diff;

public interface ICodeDiffParserFactory
{
    ICodeDiffParser Create(CodeDiffType codeDiffType);
}
