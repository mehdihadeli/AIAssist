using TreeSitter.Bindings.CustomTypes;
using TreeSitter.Bindings.Go;
using TreeSitter.Bindings.Utilities;
using static TreeSitter.Bindings.TSBindingsParser;

namespace TreeSitter.Bindings.UnitTests;

public class GoGrammarTests
{
    [Fact]
    public void TestGoGrammar()
    {
        unsafe
        {
            var parser = parser_new();
            parser_set_language(parser, TSBindingsGo.tree_sitter_go());

            // Sample Go source code
            GeneratedCString sourceCode = new GeneratedCString(
                "package main\nfunc main() { println(\"Hello, world!\") }"
            );
            TSTree* goTree = parser_parse_string(parser, null, sourceCode, (uint)sourceCode.Length);
            Assert.False(goTree is null);
            TSNode goRootNode = tree_root_node(goTree);

            string goExpression = new string(node_string(goRootNode));

            Assert.NotEmpty(goExpression);
        }
    }
}
