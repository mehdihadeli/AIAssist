using AIAssistant.Models;

namespace AIAssistant.Contracts.Diff;

public interface ICodeDiffManager
{
    IList<FileChange> ExtractFileChanges(string diff);
    void ApplyChanges(IList<FileChange> changes, string contextWorkingDirectory);
}
