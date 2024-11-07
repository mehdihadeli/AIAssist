using AIAssistant.Contracts;
using AIAssistant.Contracts.CodeAssist;

namespace AIAssistant.Services.CodeAssistStrategies;

public class TreeSitterCodeAssistSummary(CodeLoaderService codeLoaderService, ICodeFileMapService codeFileMapService)
    : ICodeAssist
{
    public Task LoadCodeFiles(string contextWorkingDirectory, IList<string>? codeFiles)
    {
        var codeCaptures = codeLoaderService.LoadTreeSitterCodeCaptures(contextWorkingDirectory);

        return Task.CompletedTask;
    }

    public IAsyncEnumerable<string> QueryChatCompletionAsync(string userQuery)
    {
        throw new NotImplementedException();
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
