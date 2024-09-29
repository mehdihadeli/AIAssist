namespace TreeSitter.Bindings;

public unsafe partial struct TSQueryMatch
{
    [NativeTypeName("uint32_t")]
    public uint id;

    [NativeTypeName("uint16_t")]
    public ushort pattern_index;

    [NativeTypeName("uint16_t")]
    public ushort capture_count;

    [NativeTypeName("const TSQueryCapture *")]
    public TSQueryCapture* captures;
}
