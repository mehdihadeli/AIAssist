using AIAssistant.Models;

namespace AIAssistant.Contracts.Diff;

public interface ICodeDiffManager
{
    public void ApplyChanges(IList<DiffResult> diffResults, string contextWorkingDirectory);
    IList<DiffResult> ParseDiffResults(string diffContent, string contextWorkingDirectory);
}
