namespace TreeSitter.Bindings;

public partial struct TSRange
{
    public TSPoint start_point;

    public TSPoint end_point;

    [NativeTypeName("uint32_t")]
    public uint start_byte;

    [NativeTypeName("uint32_t")]
    public uint end_byte;
}
