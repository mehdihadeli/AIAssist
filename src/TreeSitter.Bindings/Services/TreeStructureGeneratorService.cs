using System.Text;
using BuildingBlocks.Utils;
using TreeSitter.Bindings.Contracts;
using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace TreeSitter.Bindings.Services;

public class TreeStructureGeneratorService : ITreeStructureGeneratorService
{
    public string GenerateOriginalCodeTree(string originalCode, string relativePath)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{relativePath.NormalizePath()}:");
        WriteRootCodeLine(sb, "│   ", originalCode);

        return sb.ToString();
    }

    public string GenerateTreeSitter(IList<DefinitionCaptureItem> definitionItems, bool isFull)
    {
        var sb = new StringBuilder();

        var relativePath = definitionItems.FirstOrDefault()?.RelativePath.NormalizePath() ?? "Unknown File";

        sb.AppendLine($"{relativePath}:");
        sb.AppendLine("│");

        var groupedItems = definitionItems
            .GroupBy(item => item.CaptureKey)
            .Select(group => new { CaptureKey = group.Key, Values = group.ToList() })
            .Where(x =>
                x.Values.All(v =>
                    (!string.IsNullOrWhiteSpace(v.CodeChunk) && !isFull)
                    || (!string.IsNullOrWhiteSpace(v.Definition) && isFull)
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
            var code = !string.IsNullOrWhiteSpace(item.CodeChunk) ? item.CodeChunk : item.Definition;
            if (string.IsNullOrWhiteSpace(code))
                continue;

            // Add first line (signature or declaration) with current indentation
            WriteChildrenCodeLine(sb, indent, code);
        }

        // Add omitted code indicator if there are no further child nodes
        if (items.Count != 0)
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
        // Split lines into an array while preserving leading whitespace and handling blank lines
        var lines = itemDefinition
            .Split('\n')
            .Select(line => line.TrimEnd()) // Retain leading whitespace but trim trailing
            .ToArray();

        if (lines.Length == 0)
            return;

        // Write the first line (e.g., method signature or declaration) with a tree branch
        sb.AppendLine($"{indent}├── {lines[0]}");

        // Apply additional indentation to method bodies or any nested blocks
        string lineIndent = indent + "│   ";
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                // Preserve blank lines with appropriate tree indentation
                sb.AppendLine(lineIndent);
            }
            else
            {
                // Indent content lines correctly under the method or block
                sb.AppendLine($"{lineIndent}{lines[i]}");
            }
        }
    }

    private static void WriteRootCodeLine(StringBuilder sb, string indent, string originalCode)
    {
        sb.AppendLine("├── code:");
        WriteChildrenCodeLine(sb, indent, originalCode);
    }
}
