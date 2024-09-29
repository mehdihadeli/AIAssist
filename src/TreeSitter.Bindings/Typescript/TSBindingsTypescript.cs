using System.Runtime.InteropServices;

namespace TreeSitter.Bindings.Typescript;

public static unsafe partial class TSBindingsTypescript
{
    [DllImport("tree-sitter-typescript", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("const TSLanguage *")]
    public static extern TSLanguage* tree_sitter_typescript();
}
