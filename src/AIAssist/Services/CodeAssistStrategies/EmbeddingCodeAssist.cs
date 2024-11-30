using AIAssist.Chat.Models;
using AIAssist.Contracts;
using AIAssist.Contracts.CodeAssist;
using AIAssist.Models;
using AIAssist.Models.Options;
using BuildingBlocks.SpectreConsole.Contracts;
using BuildingBlocks.Utils;
using Humanizer;
using Microsoft.Extensions.Options;
using Spectre.Console;
using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssist.Services.CodeAssistStrategies;

public class EmbeddingCodeAssist(
    IEmbeddingService embeddingService,
    ILLMClientManager llmClientManager,
    ISpectreUtilities spectreUtilities,
    IChatSessionManager chatSessionManager,
    IOptions<AppOptions> appOptions,
    IContextService contextService,
    IPromptManager promptManager
) : ICodeAssist
{
    public async Task LoadInitCodeFiles(string contextWorkingDirectory, IList<string>? codeFiles)
    {
        contextService.AddContextFolder(contextWorkingDirectory);
        contextService.AddOrUpdateFiles(codeFiles);

        var session = chatSessionManager.GetCurrentActiveSession();
        var files = contextService.GetAllFiles();
        var codeFileMaps = files.Select(x => x.CodeFileMap).ToList();

        await AddOrUpdateEmbeddingsForFiles(codeFileMaps, session);
    }

    public async Task AddOrUpdateCodeFiles(IList<string>? codeFiles)
    {
        if (codeFiles is null || codeFiles.Count == 0)
            return;

        contextService.AddOrUpdateFiles(codeFiles);

        var session = chatSessionManager.GetCurrentActiveSession();
        var codeFileMaps = contextService.GetFiles(codeFiles).Select(x => x.CodeFileMap).ToList();

        await AddOrUpdateEmbeddingsForFiles(codeFileMaps, session);
    }

    public Task<IEnumerable<string>> GetCodeTreeContents(IList<string>? codeFiles)
    {
        if (codeFiles is null || !codeFiles.Any())
            return Task.FromResult(Enumerable.Empty<string>());

        var session = chatSessionManager.GetCurrentActiveSession();

        var filesTreeToUpdate = embeddingService
            .QueryByFilter(
                session,
                doc =>
                    codeFiles
                        .Select(FilesUtilities.NormalizePath)
                        .Contains(doc.Metadata[nameof(CodeEmbedding.RelativeFilePath).Camelize()].NormalizePath())
            )
            .Select(x => x.TreeOriginalCode)
            .Select(x => promptManager.AddCodeBlock(x));

        return Task.FromResult(filesTreeToUpdate);
    }

    public async IAsyncEnumerable<string?> QueryChatCompletionAsync(string userQuery)
    {
        var session = chatSessionManager.GetCurrentActiveSession();
        var relatedEmbeddingsResult = await embeddingService.GetRelatedEmbeddings(userQuery, session);

        if (appOptions.Value.PrintCostEnabled)
        {
            PrintEmbeddingCost(relatedEmbeddingsResult.TotalTokensCount, relatedEmbeddingsResult.TotalCost);
        }

        // Prepare context from relevant code snippets
        var embeddingOriginalTreeCodes = relatedEmbeddingsResult
            .CodeEmbeddings.Select(x => x.TreeOriginalCode)
            .ToList();

        var systemPrompt = promptManager.GetSystemPrompt(
            embeddingOriginalTreeCodes,
            llmClientManager.ChatModel.CodeAssistType,
            llmClientManager.ChatModel.CodeDiffType
        );

        // Generate a response from the language model (e.g., OpenAI or Llama)
        var completionStreams = llmClientManager.GetCompletionStreamAsync(
            userQuery: userQuery,
            systemPrompt: systemPrompt
        );

        await foreach (var streamItem in completionStreams)
        {
            if (streamItem is null)
                continue;

            yield return streamItem;
        }
    }

    private async Task AddOrUpdateEmbeddingsForFiles(IList<CodeFileMap> codeFileMaps, ChatSession chatSession)
    {
        // generate embeddings data with using llms embeddings apis
        // https://ollama.com/blog/embedding-models
        // https://github.com/microsoft/semantic-kernel/blob/main/dotnet/notebooks/06-memory-and-embeddings.ipynb
        // https://github.com/chroma-core/chroma
        var filesEmbedding = await embeddingService.AddOrUpdateEmbeddingsForFiles(codeFileMaps, chatSession);

        PrintEmbeddingCost(filesEmbedding.TotalTokensCount, filesEmbedding.TotalCost);
    }

    private void PrintEmbeddingCost(int totalCount, decimal totalCost)
    {
        spectreUtilities.InformationText(
            message: $"Total Embedding Tokens: {totalCount.FormatCommas()} | Total Embedding Cost: ${totalCost.FormatCommas()}",
            justify: Justify.Right
        );
        spectreUtilities.WriteRule();
    }
}
