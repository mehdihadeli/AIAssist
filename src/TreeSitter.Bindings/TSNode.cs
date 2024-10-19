using System.Runtime.CompilerServices;
using TreeSitter.Bindings.CustomTypes;

namespace TreeSitter.Bindings;

public unsafe partial struct TSNode
{
    [NativeTypeName("uint32_t[4]")]
    public _context_e__FixedBuffer context;

    [NativeTypeName("const void *")]
    public void* id;

    [NativeTypeName("const TSTree *")]
    public TSTree* tree;

    [InlineArray(4)]
    public partial struct _context_e__FixedBuffer
    {
        public uint e0;
    }
}
