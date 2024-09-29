using System.Runtime.InteropServices;

namespace TreeSitter.Bindings.Java;

public static unsafe partial class TSBindingsJava
{
    [DllImport("tree-sitter-java", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("const TSLanguage *")]
    public static extern TSLanguage* tree_sitter_java();
}
