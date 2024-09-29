using System.Runtime.InteropServices;

namespace TreeSitter.Bindings.Python;

public static unsafe partial class TSBindingsPython
{
    [DllImport("tree-sitter-python", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("const TSLanguage *")]
    public static extern TSLanguage* tree_sitter_python();
}
