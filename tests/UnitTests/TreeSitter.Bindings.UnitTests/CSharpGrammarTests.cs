using TreeSitter.Bindings.Csharp;
using TreeSitter.Bindings.CustomTypes;
using TreeSitter.Bindings.Utilities;
using static TreeSitter.Bindings.TSBindingsParser;

namespace TreeSitter.Bindings.UnitTests;

public class CSharpGrammarTests
{
    [Fact]
    public void TestCSharpGrammar()
    {
        unsafe
        {
            var parser = parser_new();
            parser_set_language(parser, TSBindingsCsharp.tree_sitter_c_sharp());

            // Sample C# source code
            GeneratedCString sourceCode = new GeneratedCString(
                "using System;\nclass Program\n{\n    static void Main()\n    {\n        Console.WriteLine(\"Hello, world!\");\n    }\n}"
            );
            TSTree* csharpTree = parser_parse_string(parser, null, sourceCode, (uint)sourceCode.Length);

            // Ensure the tree is not null
            Assert.False(csharpTree is null);

            // Get the root node
            TSNode csharpRootNode = tree_root_node(csharpTree);

            // Convert the root node to string for verification
            string csharpExpression = new string(node_string(csharpRootNode));

            // Ensure the expression is not empty
            Assert.NotEmpty(csharpExpression);
        }
    }
}
