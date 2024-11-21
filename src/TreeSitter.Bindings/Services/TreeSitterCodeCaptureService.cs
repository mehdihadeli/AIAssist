using System.Text;
using BuildingBlocks.Extensions;
using BuildingBlocks.Types;
using BuildingBlocks.Utils;
using TreeSitter.Bindings.Contracts;
using TreeSitter.Bindings.CustomTypes.TreeParser;
using TreeSitter.Bindings.Utilities;
using static TreeSitter.Bindings.TSBindingsParser;

namespace TreeSitter.Bindings.Services;

// ref: https://tree-sitter.github.io/tree-sitter/using-parsers

public class TreeSitterCodeCaptureService(ITreeSitterParser treeSitterParser) : ITreeSitterCodeCaptureService
{
    public IReadOnlyList<DefinitionCaptureItem> CreateTreeSitterMap(IEnumerable<CodeFile> codeFiles)
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
                        CaptureKey = "definition.code",
                        RelativePath = codeFile.RelativePath.NormalizePath(),
                        CaptureValue = codeFile.Code,
                        CodeChunk = null,
                        OriginalCode = codeFile.Code,
                        Definition = codeFile.Code,
                    }
                );

                capturesResult.DefinitionCaptureItems.Add(
                    new DefinitionCaptureItem
                    {
                        CaptureKey = "name.code",
                        RelativePath = codeFile.RelativePath.NormalizePath(),
                        CaptureValue = CodeHelper.GetLinesOfInterest(codeFile.Code, 1),
                        CodeChunk = CodeHelper.GetLinesOfInterest(codeFile.Code, 1),
                        OriginalCode = codeFile.Code,
                        Definition = null,
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
                            case { } key when key.StartsWith("reference_name."):
                                capturesResult.ReferenceCaptureItems.Add(
                                    new ReferenceCaptureItem
                                    {
                                        CaptureKey = captureKey,
                                        RelativePath = codeFile.RelativePath.NormalizePath(),
                                        CaptureValue = captureValue,
                                        CodeChunk = CodeHelper.GetLinesOfInterest(codeFile.Code, startLine),
                                        Definition = null,
                                        OriginalCode = codeFile.Code,
                                    }
                                );

                                break;
                            case { } key when key.StartsWith("reference."):
                                string referenceChunk = CodeHelper.GetChunkOfLines(codeFile.Code, startLine, endLine);
                                capturesResult.ReferenceCaptureItems.Add(
                                    new ReferenceCaptureItem
                                    {
                                        CaptureKey = captureKey,
                                        RelativePath = codeFile.RelativePath.NormalizePath(),
                                        CaptureValue = captureValue,
                                        CodeChunk = null,
                                        Definition = captureValue,
                                        OriginalCode = codeFile.Code,
                                    }
                                );
                                break;
                            case { } key when key.StartsWith("name."):
                                string codeChunk = CodeHelper.GetLinesOfInterest(codeFile.Code, startLine);
                                if (language == ProgrammingLanguage.Csharp && key == "name.method")
                                {
                                    codeChunk = CodeHelper.GetLinesOfInterest(
                                        codeFile.Code,
                                        startLine,
                                        endPatterns: [")"],
                                        stopPatterns: ["{"]
                                    );
                                }

                                capturesResult.DefinitionCaptureItems.Add(
                                    new DefinitionCaptureItem
                                    {
                                        CaptureKey = captureKey,
                                        RelativePath = codeFile.RelativePath.NormalizePath(),
                                        CaptureValue = captureValue,
                                        CodeChunk = codeChunk,
                                        Definition = null,
                                        OriginalCode = codeFile.Code,
                                    }
                                );

                                break;
                            case { } key when key.StartsWith("definition."):
                                string definitionChunk = CodeHelper.GetChunkOfLines(codeFile.Code, startLine, endLine);
                                capturesResult.DefinitionCaptureItems.Add(
                                    new DefinitionCaptureItem
                                    {
                                        CaptureKey = captureKey,
                                        RelativePath = codeFile.RelativePath.NormalizePath(),
                                        CaptureValue = captureValue,
                                        CodeChunk = null,
                                        Definition = captureValue,
                                        OriginalCode = codeFile.Code,
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

        LinkReferencesToDefinitions(capturesResult);

        return capturesResult.DefinitionCaptureItems.ToList().AsReadOnly();
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

    private static void LinkReferencesToDefinitions(CapturesResult capturesResult)
    {
        var definitions = capturesResult.DefinitionCaptureItems;

        foreach (var definition in definitions)
        {
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
                            RelativePath = x.RelativePath.NormalizePath(),
                            ReferencedValue = x.CaptureValue,
                            ReferencedUsage = x.CaptureKey,
                        }
                    );
                });
            }
        }
    }
}
