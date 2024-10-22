using System.Text;
using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace TreeSitter.Bindings.Utilities;

public static class TreeGenerator
{
    public static string GenerateSummarizeCode(IList<DefinitionCaptureItem> definitionItems)
    {
        var sb = new StringBuilder();

        var relativePath = definitionItems.FirstOrDefault()?.RelativePath ?? "Unknown File";

        // Add the relative path as the root node with "⋮..." to indicate omitted content
        sb.AppendLine($"{relativePath}:");
        sb.AppendLine("⋮...");

        var groupedByCaptureKey = definitionItems.GroupBy(item => item.CaptureKey).OrderBy(g => g.Key);

        foreach (var captureKeyGroup in groupedByCaptureKey)
        {
            // Add the CaptureKey as a top-level tree node
            sb.AppendLine($"├── {captureKeyGroup.Key}:");

            // Recursively add children for each CaptureValue under this CaptureKey
            AddChildItems(sb, captureKeyGroup.ToList(), "│   ", isDefinition: false);
        }

        return sb.ToString();
    }

    public static string GenerateFullCode(IList<DefinitionCaptureItem> definitionItems)
    {
        var sb = new StringBuilder();

        var relativePath = definitionItems.FirstOrDefault()?.RelativePath ?? "Unknown File";

        // Add the relative path as the root node with "⋮..." to indicate omitted content
        sb.AppendLine($"{relativePath}:");
        sb.AppendLine("⋮...");

        var groupedByCaptureKey = definitionItems.GroupBy(item => item.CaptureKey).OrderBy(g => g.Key);

        foreach (var captureKeyGroup in groupedByCaptureKey)
        {
            // Add the CaptureKey as a top-level tree node
            sb.AppendLine($"├── {captureKeyGroup.Key}:");

            // Recursively add children for each CaptureValue under this CaptureKey
            AddChildItems(sb, captureKeyGroup.ToList(), "│   ", isDefinition: true);
        }

        return sb.ToString();
    }

    private static void AddChildItems(
        StringBuilder sb,
        List<DefinitionCaptureItem> items,
        string indent,
        bool isDefinition
    )
    {
        // Group items by `Definition` or `CodeChunk` to add them as child nodes
        var groupedByCaptureValue = items
            // for name type we get code-chunk from `item.CodeChunk` which is contained the start-line of captured code and for capturing definition we use `item.Definition` as tree-sitter capture definition.
            .GroupBy(item => !isDefinition ? item.CodeChunk : item.Definition)
            .OrderBy(g => g.Key);

        foreach (var valueGroup in groupedByCaptureValue)
        {
            // Add the `Definition` or `CodeChunk` as a child node with indentation
            var lines = valueGroup.Key.Split('\n');
            sb.AppendLine($"{indent}├── {lines[0].Trim()}");

            // Add the rest of the code block with the appropriate vertical bar
            for (int i = 1; i < lines.Length; i++)
            {
                sb.AppendLine($"{indent}│   {lines[i].Trim()}");
            }
        }

        // Add omitted code indicator if there are no further child nodes
        if (items.Count > 0)
        {
            sb.AppendLine($"{indent}⋮...");
        }
    }
}
