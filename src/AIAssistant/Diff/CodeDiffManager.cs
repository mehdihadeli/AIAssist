using AIAssistant.Contracts;
using AIAssistant.Contracts.Diff;
using AIAssistant.Models;

namespace AIAssistant.Diff;

public class CodeDiffManager(ICodeDiffParser codeDiffParser, ICodeDiffUpdater codeDiffUpdater) : ICodeDiffManager
{
    public IList<FileChange> ExtractFileChanges(string diff)
    {
        return codeDiffParser.ExtractFileChanges(diff);
    }

    public void ApplyChanges(IList<FileChange> changes, string contextWorkingDirectory)
    {
        codeDiffUpdater.ApplyChanges(changes, contextWorkingDirectory);
    }
}
