using AIAssist.Models;

namespace AIAssist.Contracts.Diff;

public interface ICodeDiffParser
{
    public IList<DiffResult> ParseDiffResults(string diffContent, string contextWorkingDirectory);
}
