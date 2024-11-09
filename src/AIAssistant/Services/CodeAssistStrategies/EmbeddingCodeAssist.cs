using AIAssistant.Chat.Models;
using AIAssistant.Contracts;
using AIAssistant.Contracts.CodeAssist;
using AIAssistant.Models;
using AIAssistant.Models.Options;
using AIAssistant.Prompts;
using BuildingBlocks.SpectreConsole.Contracts;
using BuildingBlocks.Utils;
using Humanizer;
using Microsoft.Extensions.Options;
using Spectre.Console;
using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssistant.Services.CodeAssistStrategies;

public class EmbeddingCodeAssist(
    IEmbeddingService embeddingService,
    ICodeFileTreeGeneratorService codeFileTreeGeneratorService,
    ILLMClientManager llmClientManager,
    ISpectreUtilities spectreUtilities,
    IChatSessionManager chatSessionManager,
    IOptions<AppOptions> appOptions,
    IPromptCache promptCache
) : ICodeAssist
{
    public async Task LoadInitCodeFiles(string contextWorkingDirectory, IList<string>? codeFiles)
    {
        var codeFilesMap = codeFileTreeGeneratorService
            .GetOrAddCodeTreeMapFromFiles(contextWorkingDirectory, codeFiles)
            .ToList();

        if (codeFilesMap is null || codeFilesMap.Count == 0)
            throw new Exception("Not found any files to load.");

        var session = chatSessionManager.GetCurrentActiveSession();

        await AddOrUpdateCodeFilesToCache(codeFilesMap, session);
    }

    public async Task AddOrUpdateCodeFilesToCache(IList<string>? codeFiles)
    {
        if (codeFiles is null || !codeFiles.Any())
            return;

        var session = chatSessionManager.GetCurrentActiveSession();

        // Update tree code map
        var updatedCodeFilesMap = codeFileTreeGeneratorService.AddOrUpdateCodeTreeMapFromFiles(codeFiles).ToList();

        await AddOrUpdateCodeFilesToCache(updatedCodeFilesMap, session);
    }

    public Task<IEnumerable<string>> GetCodeTreeContentsFromCache(IList<string>? codeFiles)
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
            .Select(x => SharedPrompts.AddCodeBlock(x));

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
        var codeContext = SharedPrompts.CreateLLMContext(relatedEmbeddingsResult.CodeEmbeddings);

        var systemCodeAssistPrompt = promptCache.GetPrompt(
            CommandType.Code,
            llmClientManager.ChatModel.ModelOption.CodeDiffType,
            new { codeContext = codeContext }
        );

        // Generate a response from the language model (e.g., OpenAI or Llama)
        var completionStreams = llmClientManager.GetCompletionStreamAsync(
            userQuery: userQuery,
            systemContext: systemCodeAssistPrompt
        );

        await foreach (var streamItem in completionStreams)
        {
            yield return streamItem;
        }
    }

    private async Task AddOrUpdateCodeFilesToCache(IList<CodeFileMap> codeFileMaps, ChatSession chatSession)
    {
        // generate embeddings data with using llms embeddings apis
        // https://ollama.com/blog/embedding-models
        // https://github.com/microsoft/semantic-kernel/blob/main/dotnet/notebooks/06-memory-and-embeddings.ipynb
        // https://github.com/chroma-core/chroma
        var relatedEmbeddingsResult = await embeddingService.AddOrUpdateEmbeddingsForFiles(codeFileMaps, chatSession);

        PrintEmbeddingCost(relatedEmbeddingsResult.TotalTokensCount, relatedEmbeddingsResult.TotalCost);
    }

    private void PrintEmbeddingCost(int totalCount, decimal totalCost)
    {
        spectreUtilities.InformationText(
            message: $"Total Embedding Tokens: {totalCount.FormatCommas()} | Total Embedding Cost: ${totalCost.FormatCommas()}",
            justify: Justify.Right
        );
    }
}
