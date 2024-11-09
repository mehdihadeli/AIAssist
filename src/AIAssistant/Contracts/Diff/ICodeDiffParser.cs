using AIAssistant.Models;

namespace AIAssistant.Contracts.Diff;

public interface ICodeDiffParser
{
    IList<FileChange> GetFileChanges(string diff);
}
