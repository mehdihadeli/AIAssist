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
        sb.AppendLine("│");

        var indent = "│   ";
        sb.AppendLine("├── definition: ");
        WriteMultiLine(sb, indent, originalCode);

        return sb.ToString();
    }

    public string GenerateTreeSitter(IList<DefinitionCapture> definitionCaptures, bool isFull)
    {
        var sb = new StringBuilder();

        var normalizedRelativePath = definitionCaptures.First().RelativePath.NormalizePath();

        // Start tree with file path
        sb.AppendLine($"{normalizedRelativePath}:");
        sb.AppendLine("│");

        // Group definition items by their capture group
        var groupedItems = definitionCaptures
            .GroupBy(item => item.CaptureGroup)
            .Select(group => new { CaptureKey = group.Key, Items = group.ToList() })
            .OrderBy(group => group.CaptureKey)
            .ToList();

        foreach (var group in groupedItems)
        {
            foreach (var definitionCapture in group.Items)
            {
                var captureGroup = group.CaptureKey;
                sb.AppendLine($"├── {captureGroup}:");

                AddCaptureItems(sb, definitionCapture.CaptureItems, "│   ", isFull);

                if (!isFull)
                {
                    AddSigniture(sb, definitionCapture, "│   ");
                }
            }
        }

        return sb.ToString();
    }

    private static void AddCaptureItems(
        StringBuilder sb,
        IList<DefinitionCaptureItem> items,
        string indent,
        bool isFull
    )
    {
        foreach (var item in items)
        {
            var normalizedKeyProperty = GetNormalizedKeyProperty(item.CaptureKey);

            if (normalizedKeyProperty.StartsWith("definition") && !isFull)
            {
                continue;
            }

            if (normalizedKeyProperty.StartsWith("definition") && isFull)
            {
                sb.AppendLine($"{indent}├── definition: ");
                WriteMultiLine(sb, indent, item.CaptureValue);
                continue;
            }

            WriteSingleLine(sb, normalizedKeyProperty, item.CaptureValue, indent);
        }
    }

    private static void AddSigniture(StringBuilder sb, DefinitionCapture item, string indent)
    {
        if (string.IsNullOrWhiteSpace(item.Signiture))
            return;

        WriteSingleLine(sb, "signiture", item.Signiture, indent);
    }

    private static void WriteSingleLine(
        StringBuilder sb,
        string normalizedCaptureKey,
        string captureValue,
        string indent
    )
    {
        sb.AppendLine($"{indent}├── {normalizedCaptureKey}: {captureValue}");
    }

    private static void WriteMultiLine(StringBuilder sb, string indent, string itemDefinition)
    {
        // Split lines into an array while preserving leading whitespace and handling blank lines
        var lines = itemDefinition
            .Split('\n')
            .Select(line => line.TrimEnd()) // Retain leading whitespace but trim trailing
            .ToArray();

        if (lines.Length == 0)
            return;

        // Apply additional indentation to method bodies or any nested blocks
        string lineIndent = indent + "│   ";
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                // Preserve blank lines with appropriate tree indentation
                sb.AppendLine(lineIndent);
            }
            else
            {
                // Indent content lines correctly under the method or block
                sb.AppendLine($"{lineIndent}{line}");
            }
        }
    }

    private static string GetNormalizedKeyProperty(string key)
    {
        var index = key.IndexOf('.', StringComparison.Ordinal);
        return index >= 0 ? key.Substring(0, index) : key;
    }
}
