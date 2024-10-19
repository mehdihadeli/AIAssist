using System.Text;
using BuildingBlocks.Extensions;
using BuildingBlocks.Types;
using TreeSitter.Bindings.CustomTypes.TreeParser;
using static TreeSitter.Bindings.TSBindingsParser;
using static TreeSitter.Bindings.Utilities.TreeSitterParser;

namespace TreeSitter.Bindings.Utilities;

// ref: https://tree-sitter.github.io/tree-sitter/using-parsers

public class CodeFileMappings
{
    public IReadOnlyList<CodeFileMap> CreateTreeSitterMap(IEnumerable<CodeFile> codeFiles)
    {
        var capturesResult = new CapturesResult();

        foreach (var codeFile in codeFiles)
        {
            var language = codeFile.RelativePath.GetLanguageFromFilePath();

            if (language is null)
            {
                capturesResult.DefinitionCaptureItems.Add(
                    new DefinitionCaptureItem
                    {
                        CaptureKey = "full definition",
                        RelativePath = codeFile.RelativePath,
                        CaptureValue = codeFile.Code,
                        CodeChunk = codeFile.Code,
                        OriginalCode = codeFile.Code,
                        CaptureType = CaptureType.Definition,
                    }
                );
                capturesResult.DefinitionCaptureItems.Add(
                    new DefinitionCaptureItem
                    {
                        CaptureKey = "summary",
                        RelativePath = codeFile.RelativePath,
                        CaptureValue = CodeHelper.GetLinesOfInterest(codeFile.Code, 1),
                        CodeChunk = CodeHelper.GetLinesOfInterest(codeFile.Code, 1),
                        OriginalCode = codeFile.Code,
                        CaptureType = CaptureType.Name,
                    }
                );

                continue;
            }

            unsafe
            {
                var parser = GetParser(language.Value);
                var tree = GetCodeTree(parser, codeFile.Code);
                var defaultQuery = GetLanguageDefaultQuery(language.Value);

                var rootNode = GetRootNode(tree);
                var queryCursor = query_cursor_new();
                query_cursor_exec(queryCursor, defaultQuery, rootNode);

                TSQueryMatch match;
                var captureTags = new List<CaptureNode>();

                while (query_cursor_next_match(queryCursor, &match))
                {
                    // Populate the dictionary by capture Name
                    for (int i = 0; i < match.capture_count; i++)
                    {
                        var capture = match.captures[i];
                        var node = capture.node;

                        uint length = 0;

                        sbyte* captureNamePtr = query_capture_name_for_id(defaultQuery, capture.index, &length);

                        string captureName = new GeneratedCString(captureNamePtr);

                        if (captureTags.All(x => x.CaptureKey != captureName))
                            captureTags.Add(new CaptureNode { CaptureKey = captureName, Values = { node } });
                        else
                        {
                            var captureNode = captureTags.SingleOrDefault(x => x.CaptureKey == captureName);

                            captureNode?.Values.Add(node);
                        }
                    }
                }

                // Convert code to byte array for extracting matched code using byte positions
                byte[] byteArrayCode = Encoding.UTF8.GetBytes(codeFile.Code);

                foreach (var (captureKey, captureValues) in captureTags.ToDictionary(x => x.CaptureKey, v => v.Values))
                {
                    foreach (var tagValue in captureValues)
                    {
                        // getting start and end line number for current AST node value
                        // because tree-sitter line number starts from 0 we add 1 to the startLine and endLine
                        var startLine = (int)node_start_point(tagValue).row + 1;
                        var endLine = (int)node_end_point(tagValue).row + 1;
                        var captureValue = GetValueFromNode(byteArrayCode, tagValue);

                        switch (captureKey)
                        {
                            // We exclude references because most of the references can be found in our definitions like functions
                            case { } key when key.StartsWith("reference_name.") || key.StartsWith("reference."):
                                capturesResult.ReferenceCaptureItems.Add(
                                    new ReferenceCaptureItem
                                    {
                                        CaptureKey = captureKey,
                                        RelativePath = codeFile.RelativePath,
                                        CaptureValue = captureValue,
                                        CaptureType = key.StartsWith("reference_name.")
                                            ? CaptureType.Name
                                            : CaptureType.Definition,
                                        CodeChunk = key.StartsWith("reference_name.")
                                            ? CodeHelper.GetLinesOfInterest(codeFile.Code, startLine)
                                            : CodeHelper.GetChunkOfLines(codeFile.Code, startLine, endLine),
                                        OriginalCode = codeFile.Code,
                                    }
                                );

                                break;
                            // just names will use for creating output
                            case { } key when key.StartsWith("name.") || key.StartsWith("definition."):
                                string codeChunk = key.StartsWith("name.")
                                    ? CodeHelper.GetLinesOfInterest(codeFile.Code, startLine)
                                    : CodeHelper.GetChunkOfLines(codeFile.Code, startLine, endLine);

                                if (language == ProgrammingLanguage.Csharp && key == "name.method")
                                {
                                    codeChunk = key.StartsWith("name.")
                                        ? CodeHelper.GetLinesOfInterest(
                                            codeFile.Code,
                                            startLine,
                                            endPatterns: [")"],
                                            stopPatterns: ["{"]
                                        )
                                        : CodeHelper.GetChunkOfLines(codeFile.Code, startLine, endLine);
                                }

                                capturesResult.DefinitionCaptureItems.Add(
                                    new DefinitionCaptureItem
                                    {
                                        CaptureKey = captureKey,
                                        RelativePath = codeFile.RelativePath,
                                        CaptureValue = captureValue,
                                        CodeChunk = codeChunk,
                                        OriginalCode = codeFile.Code,
                                        CaptureType = key.StartsWith("name.")
                                            ? CaptureType.Name
                                            : CaptureType.Definition,
                                    }
                                );

                                break;
                            default:
                                break;
                        }
                    }
                }

                // cleanup resources
                query_cursor_delete(queryCursor);
                query_delete(defaultQuery);
                tree_delete(tree);
                parser_delete(parser);
            }
        }

        var codeFilesMap = GenerateCodeFileMaps(capturesResult);

        return codeFilesMap.ToList().AsReadOnly();
    }

    private static string GetValueFromNode(byte[] byteArrayCode, TSNode node)
    {
        var startByte = node_start_byte(node);
        var endByte = node_end_byte(node);

        // Fetch the matched code from the byte array based on start and end byte positions
        var matchedCode = Encoding.UTF8.GetString(byteArrayCode, (int)startByte, (int)(endByte - startByte));

        // Trim the result to remove any leading or trailing whitespace
        return matchedCode.Trim();
    }

    private static IEnumerable<CodeFileMap> GenerateCodeFileMaps(CapturesResult capturesResult)
    {
        // Link references to definitions
        var linkedDefinitions = LinkReferencesToDefinitions(capturesResult);

        // Group the linked definitions by RelativePath to create a CodeFileMap for each file
        var groupedByFile = linkedDefinitions
            .GroupBy(definition => definition.RelativePath)
            .Select(group => new CodeFileMap
            {
                RelativePath = group.Key,
                TreeSitterFullCode = GenerateFullCode(group.ToList()),
                OriginalCode = GetOriginalCode(group.ToList()),
                TreeSitterSummarizeCode = GenerateSummarizeCode(group.ToList()),
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
                    RelativePath = reference.FileName,
                    ReferencedValue = reference.Value,
                    ReferencedUsage = reference.Key,
                })
            )
            .Distinct()
            .ToList();
    }

    private static IEnumerable<DefinitionCaptureItem> LinkReferencesToDefinitions(CapturesResult capturesResult)
    {
        var definitions = capturesResult.DefinitionCaptureItems;

        foreach (var definition in definitions)
        {
            if (definition.CaptureType == CaptureType.Definition)
                continue;

            var relatedReferences = capturesResult
                .ReferenceCaptureItems.Where(reference => reference.CaptureValue == definition.CaptureValue)
                .ToList();

            if (relatedReferences.Count != 0)
            {
                relatedReferences.ForEach(x =>
                {
                    definition.DefinitionCaptureReferences.Add(
                        new DefinitionCaptureReference
                        {
                            FileName = x.RelativePath,
                            Value = x.CaptureValue,
                            Key = x.CaptureKey,
                        }
                    );
                });
            }
        }

        return definitions;
    }

    private static string GenerateSummarizeCode(List<DefinitionCaptureItem> definitionItems)
    {
        var sb = new StringBuilder();

        var relativePath = definitionItems.FirstOrDefault()?.RelativePath ?? "Unknown File";

        // Add the relative path as the root node with "⋮..." to indicate omitted content
        sb.AppendLine($"{relativePath}:");
        sb.AppendLine("⋮...");

        var groupedByCaptureKey = definitionItems
            .Where(x => x.CaptureType == CaptureType.Name)
            .GroupBy(item => item.CaptureKey)
            .OrderBy(g => g.Key);

        foreach (var captureKeyGroup in groupedByCaptureKey)
        {
            // Add the CaptureKey as a top-level tree node
            sb.AppendLine($"├── {captureKeyGroup.Key}:");

            // Recursively add children for each CaptureValue under this CaptureKey
            AddChildItems(sb, captureKeyGroup.ToList(), "│   ");
        }

        return sb.ToString();
    }

    private static string GenerateFullCode(List<DefinitionCaptureItem> definitionItems)
    {
        var sb = new StringBuilder();

        var relativePath = definitionItems.FirstOrDefault()?.RelativePath ?? "Unknown File";

        // Add the relative path as the root node with "⋮..." to indicate omitted content
        sb.AppendLine($"{relativePath}:");
        sb.AppendLine("⋮...");

        var groupedByCaptureKey = definitionItems
            .Where(x => x.CaptureType == CaptureType.Definition)
            .GroupBy(item => item.CaptureKey)
            .OrderBy(g => g.Key);

        foreach (var captureKeyGroup in groupedByCaptureKey)
        {
            // Add the CaptureKey as a top-level tree node
            sb.AppendLine($"├── {captureKeyGroup.Key}:");

            // Recursively add children for each CaptureValue under this CaptureKey
            AddChildItems(sb, captureKeyGroup.ToList(), "│   ");
        }

        return sb.ToString();
    }

    private static string GetOriginalCode(List<DefinitionCaptureItem> definitionItems)
    {
        return definitionItems.First().OriginalCode;
    }

    private static void AddChildItems(StringBuilder sb, List<DefinitionCaptureItem> items, string indent)
    {
        // Group items by `CaptureValue` or `CodeChunk` to add them as child nodes
        var groupedByCaptureValue = items
            // for name type we get code-chunk from `item.CodeChunk` which is contained the start-line of captured code and for capture yupe of definition we use `item.CaptureValue` as tree-sitter capture definition.
            .GroupBy(item => item.CaptureType == CaptureType.Name ? item.CodeChunk : item.CaptureValue)
            .OrderBy(g => g.Key);

        foreach (var valueGroup in groupedByCaptureValue)
        {
            // Add the `CaptureValue` or `CodeChunk` as a child node with indentation
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
