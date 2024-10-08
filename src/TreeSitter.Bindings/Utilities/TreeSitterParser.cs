using System.Text;
using BuildingBlocks.Extensions;
using BuildingBlocks.Types;
using TreeSitter.Bindings.Csharp;
using TreeSitter.Bindings.Go;
using TreeSitter.Bindings.Java;
using TreeSitter.Bindings.Javascript;
using TreeSitter.Bindings.Python;
using TreeSitter.Bindings.Queries;
using TreeSitter.Bindings.Typescript;
using static TreeSitter.Bindings.TSBindingsParser;

namespace TreeSitter.Bindings.Utilities;

public unsafe class TreeSitterParser
{
    public static string GetRootNodeExpression(ProgrammingLanguage language, string code)
    {
        var parser = GetParser(language);

        GeneratedCString generatedCString = new GeneratedCString(code);
        TSTree* tree = parser_parse_string(parser, null, generatedCString, (uint)code.Length);
        TSNode rootNode = tree_root_node(tree);

        string expression = new string(node_string(rootNode));

        return expression;
    }

    public static TSNode GetRootNode(ProgrammingLanguage language, string code)
    {
        var parser = GetParser(language);

        GeneratedCString generatedCString = new GeneratedCString(code);
        TSTree* tree = parser_parse_string(parser, null, generatedCString, (uint)code.Length);
        TSNode rootNode = tree_root_node(tree);

        return rootNode;
    }

    public static TSParser* GetParser(ProgrammingLanguage language)
    {
        var parser = parser_new();

        var tsLanguage = GetLanguage(language);
        parser_set_language(parser, tsLanguage);

        return parser;
    }

    public static TSTree* GetCodeTree(TSParser* parser, string code)
    {
        GeneratedCString sourceCode = new GeneratedCString(code);
        TSTree* tree = parser_parse_string(parser, null, sourceCode, (uint)code.Length);

        return tree;
    }

    public static TSNode GetRootNode(TSTree* tree)
    {
        return tree_root_node(tree);
    }

    public static TSLanguage* GetLanguage(ProgrammingLanguage programmingLanguage)
    {
        switch (programmingLanguage)
        {
            case ProgrammingLanguage.Go:
                return TSBindingsGo.tree_sitter_go();
            case ProgrammingLanguage.Csharp:
                return TSBindingsCsharp.tree_sitter_c_sharp();
            case ProgrammingLanguage.Java:
                return TSBindingsJava.tree_sitter_java();
            case ProgrammingLanguage.Javascript:
                return TSBindingsJavascript.tree_sitter_javascript();
            case ProgrammingLanguage.Python:
                return TSBindingsPython.tree_sitter_python();
            case ProgrammingLanguage.Typescript:
                return TSBindingsTypescript.tree_sitter_typescript();
            default:
                return null;
        }
    }

    public static TSQuery* GetLanguageDefaultQuery(ProgrammingLanguage programmingLanguage)
    {
        var language = GetLanguage(programmingLanguage);
        var defaultLanguageQuery = QueryManager.GetDefaultLanguageQuery(programmingLanguage);

        if (language is null)
        {
            return null;
        }

        uint errorOffset;
        TSQueryError queryError;

        var query = query_new(
            language,
            new GeneratedCString(defaultLanguageQuery),
            (uint)defaultLanguageQuery.Length,
            &errorOffset,
            &queryError
        );

        return query;
    }

    public static string GetTreeSitterIfAvailable(string code, string path)
    {
        var language = path.GetLanguageFromFilePath();
        if (language is null)
            return code;

        var parser = GetParser(language.Value);
        var tree = GetCodeTree(parser, code);
        var defaultQuery = GetLanguageDefaultQuery(language.Value);

        var rootNode = GetRootNode(tree);
        var queryCursor = query_cursor_new();
        query_cursor_exec(queryCursor, defaultQuery, rootNode);

        TSQueryMatch match;

        byte[] byteArrayCode = Encoding.UTF8.GetBytes(code);

        Dictionary<string, List<string>> items = new Dictionary<string, List<string>>();
        while (query_cursor_next_match(queryCursor, &match))
        {
            for (uint i = 0; i < match.capture_count; i++)
            {
                var capture = match.captures[i];
                var node = capture.node;

                var nodeEndStartByte = node_start_byte(node);
                var nodeEndByte = node_end_byte(node);

                string codeMatched = System.Text.Encoding.UTF8.GetString(
                    byteArrayCode,
                    (int)nodeEndStartByte,
                    (int)(nodeEndByte - nodeEndStartByte)
                );

                uint length = 0;
                // Get the capture name, like @name, @definition.class, etc.
                sbyte* captureNamePtr = query_capture_name_for_id(defaultQuery, capture.index, &length);

                // Convert the capture name to a string (length is already provided)
                string captureName = new GeneratedCString(captureNamePtr);

                if (!items.ContainsKey(captureName))
                {
                    items[captureName] = new List<string>();
                }

                items[captureName].Add(codeMatched);
            }
        }

        // Create a formatted result string by joining the items
        var result = string.Concat(
            items.Select(kv =>
            {
                var s = new StringBuilder();
                kv.Value.ForEach(v => s.Append($"\n{kv.Key}: {v}\n"));

                return s;
            })
        );

        return result;
    }
}
