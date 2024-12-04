using System.Text;
using BuildingBlocks.Extensions;
using BuildingBlocks.Utils;
using TreeSitter.Bindings.Contracts;
using TreeSitter.Bindings.CustomTypes;
using TreeSitter.Bindings.CustomTypes.TreeParser;
using TreeSitter.Bindings.Utilities;
using static TreeSitter.Bindings.TSBindingsParser;

namespace TreeSitter.Bindings.Services;

// ref: https://tree-sitter.github.io/tree-sitter/using-parsers

public class TreeSitterCodeCaptureService(ITreeSitterParser treeSitterParser) : ITreeSitterCodeCaptureService
{
    public IReadOnlyList<DefinitionCapture> CreateTreeSitterMap(IEnumerable<CodeFile> codeFiles)
    {
        var capturesResult = new CapturesResult();

        foreach (var codeFile in codeFiles)
        {
            var language = codeFile.RelativePath.GetLanguageFromFilePath();

            if (language is null)
            {
                capturesResult.DefinitionCaptureItems.Add(
                    new DefinitionCapture
                    {
                        CaptureItems = new List<DefinitionCaptureItem>
                        {
                            new()
                            {
                                CaptureKey = "name.code",
                                CaptureValue = CodeHelper.GetLinesOfInterest(codeFile.Code, 1),
                            },
                            new() { CaptureKey = "definition.code", CaptureValue = codeFile.Code },
                        },
                        RelativePath = codeFile.RelativePath.NormalizePath(),
                        Signiture = CodeHelper.GetLinesOfInterest(codeFile.Code, 1),
                        OriginalCode = codeFile.Code,
                        Definition = codeFile.Code,
                    }
                );

                continue;
            }

            unsafe
            {
                var parser = treeSitterParser.GetParser(language.Value);
                var tree = treeSitterParser.GetCodeTree(parser, codeFile.Code);
                var defaultQuery = treeSitterParser.GetLanguageDefaultQuery(language.Value);

                var rootNode = treeSitterParser.GetRootNode(tree);
                var queryCursor = query_cursor_new();
                query_cursor_exec(queryCursor, defaultQuery, rootNode);

                TSQueryMatch match;
                var captureGroupTags = new Dictionary<string, List<CaptureNode>>();

                while (query_cursor_next_match(queryCursor, &match))
                {
                    var groupKey = Guid.NewGuid().ToString();
                    // Populate the dictionary by capture Name
                    for (int i = 0; i < match.capture_count; i++)
                    {
                        var capture = match.captures[i];
                        var node = capture.node;

                        string captureKey = GetCaptureKey(defaultQuery, capture);
                        var group = GetCaptureGroupId(captureKey);

                        if (captureGroupTags.TryGetValue(groupKey, out var captureTagList))
                        {
                            captureTagList.Add(
                                new CaptureNode
                                {
                                    CaptureKey = captureKey,
                                    CaptureGroup = group,
                                    Value = node,
                                }
                            );
                        }
                        else
                        {
                            captureGroupTags.Add(
                                groupKey,
                                [
                                    new CaptureNode
                                    {
                                        CaptureKey = captureKey,
                                        CaptureGroup = group,
                                        Value = node,
                                    },
                                ]
                            );
                        }
                    }
                }

                // Convert code to byte array for extracting matched code using byte positions
                byte[] byteArrayCode = Encoding.UTF8.GetBytes(codeFile.Code);

                foreach (var (_, captureList) in captureGroupTags)
                {
                    var captureItems = captureList
                        .Select(captureNode =>
                        {
                            var startLine = (int)node_start_point(captureNode.Value).row + 1;
                            var endLine = (int)node_end_point(captureNode.Value).row + 1;
                            var captureValue = GetValueFromNode(byteArrayCode, captureNode.Value);

                            return new DefinitionCaptureItem
                            {
                                CaptureKey = captureNode.CaptureKey,
                                CaptureValue = captureValue,
                                StartLine = startLine,
                                EndLine = endLine,
                            };
                        })
                        .ToList();

                    var nameCapture = captureItems.FirstOrDefault(x => x.CaptureKey.StartsWith("name."));
                    var definitionCapture = captureItems.FirstOrDefault(x => x.CaptureKey.StartsWith("definition."));
                    var group = captureList.FirstOrDefault()?.CaptureGroup;

                    capturesResult.DefinitionCaptureItems.Add(
                        new DefinitionCapture
                        {
                            CaptureItems = captureItems,
                            RelativePath = codeFile.RelativePath.NormalizePath(),
                            CaptureGroup = group ?? string.Empty,
                            Signiture =
                                nameCapture != null
                                    ? CodeHelper.GetLinesOfInterest(codeFile.Code, nameCapture.StartLine)
                                    : nameCapture?.CaptureValue,
                            Definition =
                                definitionCapture != null
                                    ? CodeHelper.GetChunkOfLines(
                                        codeFile.Code,
                                        definitionCapture.StartLine,
                                        definitionCapture.EndLine
                                    )
                                    : definitionCapture?.CaptureValue,
                            OriginalCode = codeFile.Code,
                        }
                    );
                }
            }
        }

        return capturesResult.DefinitionCaptureItems.ToList().AsReadOnly();
    }

    private static unsafe string GetCaptureKey(TSQuery* defaultQuery, TSQueryCapture capture)
    {
        uint length;
        sbyte* captureNamePtr = query_capture_name_for_id(defaultQuery, capture.index, &length);
        string captureKey = new GeneratedCString(captureNamePtr);

        return captureKey;
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

    private static string GetCaptureGroupId(string captureKey)
    {
        var index = captureKey.IndexOf('.', StringComparison.Ordinal);
        return index >= 0 ? captureKey.Substring(index + 1) : captureKey;
    }
}
