using System.Runtime.InteropServices;

namespace TreeSitter.Bindings.Go;

public static unsafe partial class TSBindingsGo
{
    [DllImport("tree-sitter-go", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("const TSLanguage *")]
    public static extern TSLanguage* tree_sitter_go();
}
