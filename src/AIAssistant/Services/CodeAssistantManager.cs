using System.Text;
using AIAssistant.Contracts;
using AIAssistant.Diff.CodeBlock;
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

    public Task LoadCodeFiles(ChatSession chatSession, string? contextWorkingDirectory, IEnumerable<string>? codeFiles)
    {
        return _codeStrategy.LoadCodeFiles(chatSession, contextWorkingDirectory, codeFiles);
    }

    public IAsyncEnumerable<string?> QueryAsync(string userQuery)
    {
        return _codeStrategy.QueryAsync(userQuery);
    }

    public IList<CodeBlock> ParseResponseCodeBlocks(string response)
    {
        CodeBlockParser codeBlockParser = new CodeBlockParser();
        var codeBlocks = codeBlockParser.ExtractCodeBlocks(response);

        return codeBlocks;
    }

    public void ApplyChangesToFiles(IList<CodeBlock> codeBlocks)
    {
        CodeBlockCodeUpdater codeBlockCodeUpdater = new CodeBlockCodeUpdater();
        codeBlockCodeUpdater.ApplyChanges(codeBlocks);
    }

    public async Task<string> GetResponseFromStreamAsync(IAsyncEnumerable<string?> responseStream)
    {
        var responseBuilder = new StringBuilder();

        await foreach (var chunk in responseStream)
        {
            if (!string.IsNullOrEmpty(chunk))
            {
                responseBuilder.Append(chunk);
            }
        }

        return responseBuilder.ToString();
    }
}
