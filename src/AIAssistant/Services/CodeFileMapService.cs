using TreeSitter.Bindings.CustomTypes.TreeParser;
using TreeSitter.Bindings.Utilities;

namespace AIAssistant.Services;

public class CodeFileMapService
{
    public IEnumerable<CodeFileMap> GenerateCodeFileMaps(IReadOnlyList<DefinitionCaptureItem> definitions)
    {
        // Group the linked definitions by RelativePath to create a CodeFileMap for each file
        var groupedByFile = definitions
            .GroupBy(definition => definition.RelativePath)
            .Select(group => new CodeFileMap
            {
                RelativePath = group.Key,
                TreeSitterFullCode = TreeGenerator.GenerateFullCode(group.ToList()),
                OriginalCode = GetOriginalCode(group.ToList()),
                TreeSitterSummarizeCode = TreeGenerator.GenerateSummarizeCode(group.ToList()),
                ReferencedCodesMap = GenerateRelatedCodeFilesMap(group.ToList()),
            })
            .ToList();

        return groupedByFile;
    }

    private static IEnumerable<ReferencedCodeMap> GenerateRelatedCodeFilesMap(
        List<DefinitionCaptureItem> definitionItems
    )
    {
        // Collect related references for each DefinitionCaptureItem
        return definitionItems
            .SelectMany(item =>
                item.DefinitionCaptureReferences.Select(reference => new ReferencedCodeMap
                {
                    RelativePath = reference.RelativePath,
                    ReferencedValue = reference.ReferencedValue,
                    ReferencedUsage = reference.ReferencedUsage,
                })
            )
            .Distinct()
            .ToList();
    }

    private static string GetOriginalCode(List<DefinitionCaptureItem> definitionItems)
    {
        return definitionItems.First().OriginalCode;
    }
}
