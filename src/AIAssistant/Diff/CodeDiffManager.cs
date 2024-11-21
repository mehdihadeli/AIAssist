using AIAssistant.Contracts.Diff;
using AIAssistant.Models;

namespace AIAssistant.Diff;

public class CodeDiffManager(ICodeDiffUpdater codeDiffUpdater, ICodeDiffParser codeDiffParser) : ICodeDiffManager
{
    public void ApplyChanges(IList<DiffResult> diffResults, string contextWorkingDirectory)
    {
        codeDiffUpdater.ApplyChanges(diffResults, contextWorkingDirectory);
    }

    public IList<DiffResult> ParseDiffResults(string diffContent, string contextWorkingDirectory)
    {
        return codeDiffParser.ParseDiffResults(diffContent, contextWorkingDirectory);
    }
}
