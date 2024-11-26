using AIAssist.Contracts.Diff;
using AIAssist.Models;

namespace AIAssist.Diff;

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
