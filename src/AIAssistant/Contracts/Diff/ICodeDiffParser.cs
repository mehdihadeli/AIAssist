using AIAssistant.Models;

namespace AIAssistant.Contracts.Diff;

public interface ICodeDiffParser
{
    public IList<DiffResult> ParseDiffResults(string diffContent, string contextWorkingDirectory);
}
