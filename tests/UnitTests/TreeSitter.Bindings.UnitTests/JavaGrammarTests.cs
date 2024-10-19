using TreeSitter.Bindings.CustomTypes;
using TreeSitter.Bindings.Java;
using TreeSitter.Bindings.Utilities;
using static TreeSitter.Bindings.TSBindingsParser;

namespace TreeSitter.Bindings.UnitTests;

public class JavaGrammarTests
{
    [Fact]
    public void TestJavaGrammar()
    {
        unsafe
        {
            var parser = parser_new();
            parser_set_language(parser, TSBindingsJava.tree_sitter_java());

            // Sample Java source code
            GeneratedCString sourceCode = new GeneratedCString(
                "public class HelloWorld {\n    public static void main(String[] args) {\n        System.out.println(\"Hello, world!\");\n    }\n}"
            );

            TSTree* javaTree = parser_parse_string(parser, null, sourceCode, (uint)sourceCode.Length);

            Assert.False(javaTree is null);
            TSNode javaRootNode = tree_root_node(javaTree);
            string javaExpression = new string(node_string(javaRootNode));
            Assert.NotEmpty(javaExpression);
        }
    }
}
