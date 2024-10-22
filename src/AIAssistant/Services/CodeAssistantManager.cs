using System.Text;
using AIAssistant.Contracts;
using AIAssistant.Diff;
using AIAssistant.Models.Options;
using Clients.Chat.Models;
using Microsoft.Extensions.Options;

namespace AIAssistant.Services;

public class CodeAssistantManager(
    ICodeAssistStrategyFactory codeAssistStrategyFactory,
    IOptions<CodeAssistOptions> codeAssistOptions
) : ICodeAssistantManager
{
    private readonly ICodeStrategy _codeStrategy = codeAssistStrategyFactory.Create(
        codeAssistOptions.Value.CodeAssistType
    );

    public Task LoadCodeFiles(
        ChatSession chatSession,
        string contextWorkingDirectory,
        IEnumerable<string>? extraCodeFiles = null
    )
    {
        return _codeStrategy.LoadCodeFiles(chatSession, contextWorkingDirectory, extraCodeFiles);
    }

    public IAsyncEnumerable<string?> QueryAsync(string userQuery)
    {
        return _codeStrategy.QueryAsync(userQuery);
    }

    public async Task ApplyChanges(IAsyncEnumerable<string?> responseStream)
    {
        var response = await GetResponseFromStreamAsync(responseStream);

        DiffParser parser = new DiffParser();
        var changes = parser.ParseUnifiedDiff(response).ToList();

        CodeUpdater updater = new CodeUpdater();
        updater.ApplyChanges(changes);
    }

    private async Task<string> GetResponseFromStreamAsync(IAsyncEnumerable<string?> responseStream)
    {
        var responseBuilder = new StringBuilder();

        await foreach (var chunk in responseStream)
        {
            if (!string.IsNullOrEmpty(chunk))
            {
                responseBuilder.AppendLine(chunk);
            }
        }

        return responseBuilder.ToString();
    }
}
