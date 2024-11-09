using AIAssistant.Contracts;
using TreeSitter.Bindings.CustomTypes.TreeParser;
using TreeSitter.Bindings.Utilities;

namespace AIAssistant.Services;

public class CodeFileMapService : ICodeFileMapService
{
    public IEnumerable<CodeFileMap> GenerateCodeFileMaps(IReadOnlyList<DefinitionCaptureItem> definitions)
    {
        // Group the linked definitions by RelativePath to create a CodeFileMap for each file
        var groupedByFile = definitions
            .GroupBy(definition => definition.RelativePath)
            .Select(fileCapturesGroup => new CodeFileMap
            {
                RelativePath = fileCapturesGroup.Key,
                OriginalCode = GetOriginalCode(fileCapturesGroup.ToList()),
                TreeSitterFullCode = TreeGenerator.GenerateTreeSitter(fileCapturesGroup.ToList(), true),
                TreeOriginalCode = TreeGenerator.GenerateOriginalCodeTree(
                    GetOriginalCode(fileCapturesGroup.ToList()),
                    fileCapturesGroup.Key
                ),
                TreeSitterSummarizeCode = TreeGenerator.GenerateTreeSitter(fileCapturesGroup.ToList(), false),
                ReferencedCodesMap = GenerateRelatedCodeFilesMap(fileCapturesGroup.ToList()),
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
