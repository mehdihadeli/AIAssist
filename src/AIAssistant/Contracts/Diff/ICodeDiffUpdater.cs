using AIAssistant.Models;

namespace AIAssistant.Contracts.Diff;

public interface ICodeDiffUpdater
{
    public void ApplyChanges(IList<DiffResult> diffResults, string contextWorkingDirectory);
}
