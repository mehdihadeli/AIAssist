using TreeSitter.Bindings.Typescript;
using static TreeSitter.Bindings.TSBindingsParser;

namespace TreeSitter.Bindings.UnitTests;

// public class TypeScriptGrammarTests
// {
//     [Fact]
//     public void TestTypeScriptGrammar()
//     {
//         unsafe
//         {
//             var parser = parser_new();
//             parser_set_language(parser, TSBindingsTypescript.tree_sitter_typescript());
//
//             // Sample TypeScript source code
//             GeneratedCString sourceCode = new GeneratedCString("function greet(): void {\n    console.log(\"Hello, world!\");\n}\ngreet();");
//             TSTree* tsTree = parser_parse_string(parser, null, sourceCode, (uint)sourceCode.Length);
//                 
//             Assert.False(tsTree is null);
//             TSNode tsRootNode = tree_root_node(tsTree);
//             string tsExpression = new string(node_string(tsRootNode));
//             Assert.NotEmpty(tsExpression);
//         }
//     }
// }