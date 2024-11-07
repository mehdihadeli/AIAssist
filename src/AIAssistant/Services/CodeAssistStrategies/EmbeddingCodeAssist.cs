using AIAssistant.Contracts;
using AIAssistant.Contracts.CodeAssist;
using AIAssistant.Models;
using AIAssistant.Models.Options;
using BuildingBlocks.SpectreConsole.Contracts;
using BuildingBlocks.Utils;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace AIAssistant.Services.CodeAssistStrategies;

public class EmbeddingCodeAssist(
    CodeLoaderService codeLoaderService,
    EmbeddingService embeddingService,
    ICodeFileMapService codeFileMapService,
    ILLMClientManager llmClientManager,
    ISpectreUtilities spectreUtilities,
    IOptions<AppOptions> appOptions,
    IPromptCache promptCache
) : ICodeAssist
{
    public async Task LoadCodeFiles(string contextWorkingDirectory, IList<string>? codeFiles)
    {
        var treeSitterCodeCaptures = codeLoaderService.LoadTreeSitterCodeCaptures(contextWorkingDirectory, codeFiles);

        if (!treeSitterCodeCaptures.Any())
            throw new Exception("Not found any files to load.");

        var codeFilesMap = codeFileMapService.GenerateCodeFileMaps(treeSitterCodeCaptures);

        // generate embeddings data with using llms embeddings apis
        // https://ollama.com/blog/embedding-models
        // https://github.com/microsoft/semantic-kernel/blob/main/dotnet/notebooks/06-memory-and-embeddings.ipynb
        // https://github.com/chroma-core/chroma
        var relatedEmbeddingsResult = await embeddingService.AddEmbeddingsForFiles(codeFilesMap);

        PrintEmbeddingCost(relatedEmbeddingsResult.TotalTokensCount, relatedEmbeddingsResult.TotalCost);
    }

    public async IAsyncEnumerable<string?> QueryChatCompletionAsync(string userQuery)
    {
        var relatedEmbeddingsResult = await embeddingService.GetRelatedEmbeddings(userQuery);

        if (appOptions.Value.PrintCostEnabled)
        {
            PrintEmbeddingCost(relatedEmbeddingsResult.TotalTokensCount, relatedEmbeddingsResult.TotalCost);
        }

        // Prepare context from relevant code snippets
        var codeContext = embeddingService.CreateLLMContext(relatedEmbeddingsResult.CodeEmbeddings);

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

    private void PrintEmbeddingCost(int totalCount, decimal totalCost)
    {
        spectreUtilities.InformationText(
            message: $"Total Embedding Tokens: {totalCount.FormatCommas()} | Total Embedding Cost: ${totalCost.FormatCommas()}",
            justify: Justify.Right
        );
    }
}
