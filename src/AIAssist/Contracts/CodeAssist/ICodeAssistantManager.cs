using AIAssist.Models;

namespace AIAssist.Contracts.CodeAssist;

public interface ICodeAssistantManager
{
    Task LoadCodeFiles(string contextWorkingDirectory, IList<string>? codeFiles);
    IAsyncEnumerable<string?> QueryAsync(string userQuery);
    Task AddOrUpdateCodeFiles(IList<string>? codeFiles);
    Task<IEnumerable<string>> GetCodeTreeContents(IList<string>? codeFiles);
    bool CheckExtraContextForResponse(string response, out IList<string> requiredFiles);
    IList<DiffResult> ParseDiffResults(string diffContent, string contextWorkingDirectory);
    void ApplyChanges(IList<DiffResult> diffResults, string contextWorkingDirectory);
}
