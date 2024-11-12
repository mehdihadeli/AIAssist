using System.Text;
using TreeSitter.Bindings.Contracts;
using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace TreeSitter.Bindings.Services;

public class TreeStructureGeneratorService : ITreeStructureGeneratorService
{
    public string GenerateOriginalCodeTree(string originalCode, string relativePath)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{relativePath}:");
        WriteRootCodeLine(sb, "│   ", originalCode);

        return sb.ToString();
    }

    public string GenerateTreeSitter(IList<DefinitionCaptureItem> definitionItems, bool isFull)
    {
        var sb = new StringBuilder();

        var relativePath = definitionItems.FirstOrDefault()?.RelativePath ?? "Unknown File";

        // Add the relative path as the root node with "⋮..." to indicate omitted content
        sb.AppendLine($"{relativePath}:");
        sb.AppendLine("│");

        var groupedItems = definitionItems
            .GroupBy(item => item.CaptureKey)
            .Select(group => new { CaptureKey = group.Key, Values = group.ToList() })
            .Where(x =>
                x.Values.All(v =>
                    (!string.IsNullOrEmpty(v.CodeChunk) && !isFull) || (!string.IsNullOrEmpty(v.Definition) && isFull)
                )
            )
            .OrderBy(g => g.CaptureKey)
            .ToList();

        foreach (var groupedItem in groupedItems)
        {
            // Add the CaptureKey as a top-level tree node
            sb.AppendLine($"├── {GetNormalizedKeyName(groupedItem.CaptureKey)}:");
            // Recursively add children for each CaptureValue under this CaptureKey
            AddChildItems(sb, groupedItem.Values, "│   ");
        }

        return sb.ToString();
    }

    private static void AddChildItems(StringBuilder sb, List<DefinitionCaptureItem> items, string indent)
    {
        foreach (var item in items)
        {
            var code = !string.IsNullOrEmpty(item.CodeChunk) ? item.CodeChunk : item.Definition;
            if (string.IsNullOrEmpty(code))
                continue;

            WriteChildrenCodeLine(sb, indent, code);
        }

        // Add omitted code indicator if there are no further child nodes
        if (items.Count > 0)
        {
            sb.AppendLine($"{indent}⋮...");
        }
    }

    private static string GetNormalizedKeyName(string input)
    {
        string[] prefixes = { "name.", "reference.", "definition.", "reference_name" };

        foreach (var prefix in prefixes)
        {
            if (input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return input.Substring(prefix.Length);
            }
        }

        return input;
    }

    private static void WriteChildrenCodeLine(StringBuilder sb, string indent, string itemDefinition)
    {
        var lines = itemDefinition.Trim().Split('\n');

        // Write first line with bullet
        sb.AppendLine($"{indent}├── {lines[0]}");

        // Write subsequent lines with additional indentation, preserving each line's original indent
        string lineIndent = indent + "│   ";
        for (int i = 1; i < lines.Length; i++)
        {
            string formattedLine = lineIndent + lines[i];
            sb.AppendLine(formattedLine);
        }
    }

    private static void WriteRootCodeLine(StringBuilder sb, string indent, string originalCode)
    {
        sb.AppendLine("├── code:");
        WriteChildrenCodeLine(sb, indent, originalCode);
    }
}
