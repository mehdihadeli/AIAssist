using AIAssistant.Models;

namespace AIAssistant.Contracts.Diff;

public interface ICodeDiffManager
{
    IList<FileChange> GetFileChanges(string diff);
    void ApplyChanges(IList<FileChange> changes, string contextWorkingDirectory);
}
