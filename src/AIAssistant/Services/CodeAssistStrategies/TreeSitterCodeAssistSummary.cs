using System.Text;
using AIAssistant.Chat.Models;
using AIAssistant.Contracts;
using AIAssistant.Contracts.CodeAssist;
using AIAssistant.Models;
using AIAssistant.Prompts;
using BuildingBlocks.Utils;
using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssistant.Services.CodeAssistStrategies;

public class TreeSitterCodeAssistSummary(
    ICodeFileTreeGeneratorService codeFileTreeGeneratorService,
    IChatSessionManager chatSessionManager,
    IPromptCache promptCache,
    ILLMClientManager llmClientManager
) : ICodeAssist
{
    private readonly List<CodeSummary> _scopedCacheCodeSummaries = new();

    public async Task LoadInitCodeFiles(string contextWorkingDirectory, IList<string>? codeFiles)
    {
        var codeFilesMap = codeFileTreeGeneratorService
            .GetOrAddCodeTreeMapFromFiles(contextWorkingDirectory, codeFiles)
            .ToList();

        if (codeFilesMap is null || codeFilesMap.Count == 0)
            throw new Exception("Not found any files to load.");

        var session = chatSessionManager.GetCurrentActiveSession();

        await AddOrUpdateCodeFilesToCache(codeFilesMap, session, useFullCodeFile: false);
    }

    public async Task AddOrUpdateCodeFilesToCache(IList<string>? codeFiles)
    {
        if (codeFiles is null || !codeFiles.Any())
            return;

        var session = chatSessionManager.GetCurrentActiveSession();

        // Update tree code map
        var updatedCodeFilesMap = codeFileTreeGeneratorService.AddOrUpdateCodeTreeMapFromFiles(codeFiles).ToList();

        await AddOrUpdateCodeFilesToCache(updatedCodeFilesMap, session, useFullCodeFile: true);
    }

    public Task<IEnumerable<string>> GetCodeTreeContentsFromCache(IList<string>? codeFiles)
    {
        if (codeFiles is null || !codeFiles.Any())
            return Task.FromResult(Enumerable.Empty<string>());

        var filesTreeToUpdate = _scopedCacheCodeSummaries
            .Where(x => codeFiles.Select(FilesUtilities.NormalizePath).Contains(x.RelativeFilePath.NormalizePath()))
            .Select(x => x.UseFullCodeFile ? x.TreeOriginalCode : x.TreeSitterSummarizeCode)
            .Select(x => SharedPrompts.AddCodeBlock(x));

        return Task.FromResult(filesTreeToUpdate);
    }

    public async IAsyncEnumerable<string?> QueryChatCompletionAsync(string userQuery)
    {
        var codeContext = SharedPrompts.CreateLLMContext(_scopedCacheCodeSummaries);

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

        StringBuilder sb = new StringBuilder();
        await foreach (var streamItem in completionStreams)
        {
            sb.Append(streamItem);
            yield return streamItem;
        }
    }

    private Task AddOrUpdateCodeFilesToCache(
        IList<CodeFileMap> codeFileMaps,
        ChatSession chatSession,
        bool useFullCodeFile
    )
    {
        var updatedSummaries = codeFileMaps
            .Select(updatedCodeFileMap =>
            {
                var existingItem = _scopedCacheCodeSummaries.SingleOrDefault(x =>
                    x.RelativeFilePath.NormalizePath() == updatedCodeFileMap.RelativePath.NormalizePath()
                );

                // If an item exists, we update its properties
                if (existingItem is not null)
                {
                    existingItem.TreeSitterSummarizeCode = updatedCodeFileMap.TreeSitterSummarizeCode;
                    existingItem.TreeOriginalCode = updatedCodeFileMap.TreeOriginalCode;
                    existingItem.Code = updatedCodeFileMap.OriginalCode;
                    existingItem.SessionId = chatSession.SessionId;
                    existingItem.UseFullCodeFile = useFullCodeFile;
                    return existingItem;
                }

                // If the item doesn't exist, create a new one
                return new CodeSummary
                {
                    RelativeFilePath = updatedCodeFileMap.RelativePath,
                    TreeSitterSummarizeCode = updatedCodeFileMap.TreeSitterSummarizeCode,
                    TreeOriginalCode = updatedCodeFileMap.TreeOriginalCode,
                    Code = updatedCodeFileMap.OriginalCode,
                    SessionId = chatSession.SessionId,
                    UseFullCodeFile = useFullCodeFile,
                };
            })
            .ToList();

        _scopedCacheCodeSummaries.RemoveAll(x =>
            updatedSummaries.Any(y => y.RelativeFilePath.NormalizePath() == x.RelativeFilePath.NormalizePath())
        );

        _scopedCacheCodeSummaries.AddRange(updatedSummaries);

        return Task.CompletedTask;
    }

    // private static void CalculateDefinitionCaptureItemsRanks(IReadOnlyList<DefinitionCaptureItem> items)
    // {
    //     // Step 1: Calculate the rank for each RelativePath
    //     var relativePathRanks = new Dictionary<string, double>();
    //     double totalRelativePathRank = 0.0;
    //
    //     // Calculate rank contributions for each RelativePath
    //     foreach (var item in items)
    //     {
    //         string relativePath = item.RelativePath;
    //         double pathRank = 0.0;
    //         var uniqueFiles = new HashSet<string>();
    //
    //         // Calculate contributions from each reference
    //         foreach (var reference in item.DefinitionCaptureReferences)
    //         {
    //             // Base weight for each reference
    //             pathRank += 1.0;
    //
    //             // Add bonus weight if the reference is from a new file
    //             if (uniqueFiles.Add(reference.RelativePath))
    //             {
    //                 pathRank += 0.5;
    //             }
    //         }
    //
    //         // Accumulate the rank for the RelativePath
    //         if (!relativePathRanks.ContainsKey(relativePath))
    //         {
    //             relativePathRanks[relativePath] = 0.0;
    //         }
    //         relativePathRanks[relativePath] += pathRank;
    //
    //         // Update the total rank for normalization
    //         totalRelativePathRank += pathRank;
    //     }
    //
    //     // Normalize the ranks for each RelativePath so their total equals 1
    //     if (totalRelativePathRank > 0)
    //     {
    //         foreach (var key in relativePathRanks.Keys.ToList())
    //         {
    //             relativePathRanks[key] /= totalRelativePathRank;
    //         }
    //     }
    //     else
    //     {
    //         // If total rank is zero, assign equal rank to each RelativePath
    //         double equalRank = 1.0 / relativePathRanks.Count;
    //         foreach (var key in relativePathRanks.Keys.ToList())
    //         {
    //             relativePathRanks[key] = equalRank;
    //         }
    //     }
    //
    //     // Step 2: Distribute the rank for each RelativePath to the corresponding DefinitionCaptureItems
    //     var itemRanks = new Dictionary<DefinitionCaptureItem, double>();
    //     double totalItemRank = 0.0;
    //
    //     // Calculate item ranks based on RelativePath rank and reference contributions
    //     foreach (var item in items)
    //     {
    //         string relativePath = item.RelativePath;
    //         double relativePathRank = relativePathRanks.TryGetValue(relativePath, out var rank) ? rank : 0.0;
    //
    //         // Calculate rank contributions for this item based on its references
    //         double itemRank = 0.0;
    //         var uniqueFiles = new HashSet<string>();
    //
    //         foreach (var reference in item.DefinitionCaptureReferences)
    //         {
    //             // Contribution weight for each reference
    //             itemRank += 1.0;
    //
    //             // Add bonus weight if this reference is from a unique file
    //             if (uniqueFiles.Add(reference.RelativePath))
    //             {
    //                 itemRank += 0.5;
    //             }
    //         }
    //
    //         // Scale item rank by the RelativePath rank
    //         itemRank *= relativePathRank;
    //
    //         // Accumulate the total item rank
    //         itemRanks[item] = itemRank;
    //         totalItemRank += itemRank;
    //     }
    //
    //     // Step 3: Normalize the item ranks so that the sum of ranks for all DefinitionCaptureItems equals 1
    //     if (totalItemRank > 0)
    //     {
    //         foreach (var item in items)
    //         {
    //             if (itemRanks.TryGetValue(item, out double value))
    //             {
    //                 item.Rank = value / totalItemRank;
    //             }
    //             else
    //             {
    //                 item.Rank = 0.0;
    //             }
    //         }
    //     }
    //     else
    //     {
    //         // If total item rank is zero, assign equal rank to all items with references
    //         double equalRank = 1.0 / items.Count(i => i.DefinitionCaptureReferences.Count > 0);
    //         foreach (var item in items)
    //         {
    //             if (item.DefinitionCaptureReferences.Count > 0)
    //             {
    //                 item.Rank = equalRank;
    //             }
    //             else
    //             {
    //                 item.Rank = 0.0;
    //             }
    //         }
    //     }
    // }
}
