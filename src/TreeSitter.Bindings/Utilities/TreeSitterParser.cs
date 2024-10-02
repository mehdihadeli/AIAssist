using TreeSitter.Bindings.Csharp;
using TreeSitter.Bindings.Go;
using TreeSitter.Bindings.Java;
using TreeSitter.Bindings.Javascript;
using TreeSitter.Bindings.Python;
using TreeSitter.Bindings.Typescript;
using static TreeSitter.Bindings.TSBindingsParser;

namespace TreeSitter.Bindings.Utilities;

public unsafe class TreeSitterParser
{
    public static string GetRootNodeExpression(ProgrammingLanguage language, string code)
    {
        var parser = GetParser(language);

        GeneratedCString generatedCString = new GeneratedCString(code);
        TSTree* goTree = parser_parse_string(parser, null, generatedCString, (uint) code.Length);
        TSNode goRootNode = tree_root_node(goTree);
            
        string goExpression = new string(node_string(goRootNode));
            
        return goExpression;
    }

    public static TSParser* GetParser(ProgrammingLanguage language)
    {
        var parser = parser_new();

        switch (language)
        {
            case ProgrammingLanguage.Go:
                parser_set_language(parser, TSBindingsGo.tree_sitter_go());
                break;
            case ProgrammingLanguage.CSharp:
                parser_set_language(parser, TSBindingsCsharp.tree_sitter_c_sharp());
                break;
            case ProgrammingLanguage.Java:
                parser_set_language(parser, TSBindingsJava.tree_sitter_java());
                break;
            case ProgrammingLanguage.JavaScript:
                parser_set_language(parser, TSBindingsJavascript.tree_sitter_javascript());
                break;
            case ProgrammingLanguage.Python:
                parser_set_language(parser, TSBindingsPython.tree_sitter_python());
                break;
            case ProgrammingLanguage.TypeScript:
                parser_set_language(parser, TSBindingsTypescript.tree_sitter_typescript());
                break;
            default:
                throw new ArgumentException("Unsupported programming language.");
        }
        
        return parser;
    }

    public static string GetLanguageDefaultQuery(ProgrammingLanguage language)
    {
        return string.Empty;
    }
}
