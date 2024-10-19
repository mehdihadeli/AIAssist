using TreeSitter.Bindings.CustomTypes;
using TreeSitter.Bindings.Python;
using TreeSitter.Bindings.Utilities;
using static TreeSitter.Bindings.TSBindingsParser;

namespace TreeSitter.Bindings.UnitTests;

public class PythonGrammarTests
{
    [Fact]
    public void TestPythonGrammar()
    {
        unsafe
        {
            var parser = parser_new();
            parser_set_language(parser, TSBindingsPython.tree_sitter_python());

            // Sample Python source code
            GeneratedCString sourceCode = new GeneratedCString("def greet():\n    print(\"Hello, world!\")\ngreet()");
            TSTree* pythonTree = parser_parse_string(parser, null, sourceCode, (uint)sourceCode.Length);

            Assert.False(pythonTree is null);
            TSNode pythonRootNode = tree_root_node(pythonTree);
            string pythonExpression = new string(node_string(pythonRootNode));
            Assert.NotEmpty(pythonExpression);
        }
    }
}
