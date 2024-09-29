using System.Runtime.InteropServices;

namespace TreeSitter.Bindings.Javascript;

public static unsafe partial class TSBindingsJavascript
{
    [DllImport("tree-sitter-javascript", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("const TSLanguage *")]
    public static extern TSLanguage* tree_sitter_javascript();
}
