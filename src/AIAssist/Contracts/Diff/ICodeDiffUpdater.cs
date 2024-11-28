using AIAssist.Models;

namespace AIAssist.Contracts.Diff;

public interface ICodeDiffUpdater
{
    public void ApplyChanges(IList<DiffResult> diffResults, string contextWorkingDirectory);
}
