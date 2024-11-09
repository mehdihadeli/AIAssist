using BuildingBlocks.Types;
using TreeSitter.Bindings.Contracts;
using TreeSitter.Bindings.Csharp;
using TreeSitter.Bindings.CustomTypes;
using TreeSitter.Bindings.Go;
using TreeSitter.Bindings.Java;
using TreeSitter.Bindings.Javascript;
using TreeSitter.Bindings.Python;
using TreeSitter.Bindings.Queries;
using TreeSitter.Bindings.Typescript;
using static TreeSitter.Bindings.TSBindingsParser;

namespace TreeSitter.Bindings.Utilities;

public unsafe class TreeSitterParser : ITreeSitterParser
{
    public string GetRootNodeExpression(ProgrammingLanguage language, string code)
    {
        var parser = GetParser(language);

        GeneratedCString generatedCString = new GeneratedCString(code);
        TSTree* tree = parser_parse_string(parser, null, generatedCString, (uint)code.Length);
        TSNode rootNode = tree_root_node(tree);

        string expression = new string(node_string(rootNode));

        return expression;
    }

    public TSNode GetRootNode(ProgrammingLanguage language, string code)
    {
        var parser = GetParser(language);

        GeneratedCString generatedCString = new GeneratedCString(code);
        TSTree* tree = parser_parse_string(parser, null, generatedCString, (uint)code.Length);
        TSNode rootNode = tree_root_node(tree);

        return rootNode;
    }

    public TSParser* GetParser(ProgrammingLanguage language)
    {
        var parser = parser_new();

        var tsLanguage = GetLanguage(language);
        parser_set_language(parser, tsLanguage);

        return parser;
    }

    public TSTree* GetCodeTree(TSParser* parser, string code)
    {
        GeneratedCString sourceCode = new GeneratedCString(code);
        TSTree* tree = parser_parse_string(parser, null, sourceCode, (uint)code.Length);

        return tree;
    }

    public TSNode GetRootNode(TSTree* tree)
    {
        return tree_root_node(tree);
    }

    public TSLanguage* GetLanguage(ProgrammingLanguage programmingLanguage)
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

    public TSQuery* GetLanguageDefaultQuery(ProgrammingLanguage programmingLanguage)
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
}
