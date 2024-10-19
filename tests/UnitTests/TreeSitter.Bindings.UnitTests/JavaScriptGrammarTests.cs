using TreeSitter.Bindings.CustomTypes;
using TreeSitter.Bindings.Javascript;
using TreeSitter.Bindings.Utilities;
using static TreeSitter.Bindings.TSBindingsParser;

namespace TreeSitter.Bindings.UnitTests;

public class JavaScriptGrammarTests
{
    [Fact]
    public void TestJavaScriptGrammar()
    {
        unsafe
        {
            var parser = parser_new();
            parser_set_language(parser, TSBindingsJavascript.tree_sitter_javascript());

            // Sample JavaScript source code
            GeneratedCString sourceCode = new GeneratedCString(
                "function greet() {\n    console.log(\"Hello, world!\");\n}\ngreet();"
            );
            TSTree* jsTree = parser_parse_string(parser, null, sourceCode, (uint)sourceCode.Length);

            Assert.False(jsTree is null);
            TSNode jsRootNode = tree_root_node(jsTree);
            string jsExpression = new string(node_string(jsRootNode));
            Assert.NotEmpty(jsExpression);
        }
    }
}
