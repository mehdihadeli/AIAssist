using AIAssistant.Models;

namespace AIAssistant.Contracts.Diff;

public interface ICodeDiffUpdater
{
    void ApplyChanges(IEnumerable<FileChange> changes, string contextWorkingDirectory);
}
