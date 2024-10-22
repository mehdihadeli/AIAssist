using System.Text;
using BuildingBlocks.Extensions;
using BuildingBlocks.Types;
using TreeSitter.Bindings.CustomTypes.TreeParser;
using static TreeSitter.Bindings.TSBindingsParser;
using static TreeSitter.Bindings.Utilities.TreeSitterParser;

namespace TreeSitter.Bindings.Utilities;

// ref: https://tree-sitter.github.io/tree-sitter/using-parsers

public class TreeSitterCodeCaptures
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
                        CaptureKey = "code definition",
                        RelativePath = codeFile.RelativePath,
                        CaptureValue = CodeHelper.GetLinesOfInterest(codeFile.Code, 1),
                        CodeChunk = CodeHelper.GetLinesOfInterest(codeFile.Code, 1),
                        OriginalCode = codeFile.Code,
                        Definition = codeFile.Code,
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

                foreach (
                    var (captureKey, captureValues) in captureTags
                        // exclude definitions from both reference and name tags, we capture them directly with name tags
                        .Where(x => !x.CaptureKey.StartsWith("reference.") || !x.CaptureKey.StartsWith("definition."))
                        .ToDictionary(x => x.CaptureKey, v => v.Values)
                )
                {
                    int referenceIndex = 0;
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
                                var referenceKey = key.Replace(
                                    "reference_name.",
                                    "reference.",
                                    StringComparison.InvariantCultureIgnoreCase
                                );

                                var referenceNodeValues = captureTags
                                    .SingleOrDefault(x => x.CaptureKey == referenceKey)
                                    ?.Values;

                                string referenceDefinition = string.Empty;
                                if (referenceNodeValues is not null && referenceNodeValues.Any())
                                {
                                    referenceDefinition = GetValueFromNode(
                                        byteArrayCode,
                                        referenceNodeValues[referenceIndex]
                                    );
                                }

                                capturesResult.ReferenceCaptureItems.Add(
                                    new ReferenceCaptureItem
                                    {
                                        CaptureKey = captureKey,
                                        RelativePath = codeFile.RelativePath,
                                        CaptureValue = captureValue,
                                        CodeChunk = CodeHelper.GetLinesOfInterest(codeFile.Code, startLine),
                                        Definition = referenceDefinition,
                                        OriginalCode = codeFile.Code,
                                    }
                                );

                                break;
                            // just names will use for creating output
                            case { } key when key.StartsWith("name."):
                                string codeChunk = CodeHelper.GetLinesOfInterest(codeFile.Code, startLine);

                                var definitionKey = key.Replace(
                                    "name.",
                                    "definition.",
                                    StringComparison.InvariantCultureIgnoreCase
                                );

                                var definitionNodeValues = captureTags
                                    .SingleOrDefault(x => x.CaptureKey == definitionKey)
                                    ?.Values;

                                string definition = string.Empty;
                                if (definitionNodeValues is not null && definitionNodeValues.Any())
                                {
                                    foreach (var definitionNodeValue in definitionNodeValues)
                                    {
                                        definition +=
                                            GetValueFromNode(byteArrayCode, definitionNodeValue) + Environment.NewLine;
                                    }
                                }

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
                                        RelativePath = codeFile.RelativePath,
                                        CaptureValue = captureValue,
                                        CodeChunk = codeChunk,
                                        Definition = definition,
                                        OriginalCode = codeFile.Code,
                                    }
                                );

                                break;
                            default:
                                break;
                        }

                        referenceIndex++;
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
                            RelativePath = x.RelativePath,
                            ReferencedValue = x.CaptureValue,
                            ReferencedUsage = x.CaptureKey,
                        }
                    );
                });
            }
        }
    }
}
