using AIAssistant.Contracts.Diff;
using AIAssistant.Models;

namespace AIAssistant.Diff;

public class CodeDiffManager(ICodeDiffParser codeDiffParser, ICodeDiffUpdater codeDiffUpdater) : ICodeDiffManager
{
    public IList<FileChange> GetFileChanges(string diff)
    {
        return codeDiffParser.GetFileChanges(diff);
    }

    public void ApplyChanges(IList<FileChange> changes, string contextWorkingDirectory)
    {
        codeDiffUpdater.ApplyChanges(changes, contextWorkingDirectory);
    }
}
