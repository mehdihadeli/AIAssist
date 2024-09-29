using System.Runtime.CompilerServices;

namespace TreeSitter.Bindings;

public unsafe partial struct TSTreeCursor
{
    [NativeTypeName("const void *")]
    public void* tree;

    [NativeTypeName("const void *")]
    public void* id;

    [NativeTypeName("uint32_t[3]")]
    public _context_e__FixedBuffer context;

    [InlineArray(3)]
    public partial struct _context_e__FixedBuffer
    {
        public uint e0;
    }
}
