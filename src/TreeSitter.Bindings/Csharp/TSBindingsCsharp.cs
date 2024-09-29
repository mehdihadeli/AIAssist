using System.Runtime.InteropServices;

namespace TreeSitter.Bindings.Csharp;

public static unsafe partial class TSBindingsCsharp
{
    [DllImport("tree-sitter-c-sharp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("const TSLanguage *")]
    public static extern TSLanguage* tree_sitter_c_sharp();
}
