namespace TreeSitter.Bindings;

public partial struct TSInputEdit
{
    [NativeTypeName("uint32_t")]
    public uint start_byte;

    [NativeTypeName("uint32_t")]
    public uint old_end_byte;

    [NativeTypeName("uint32_t")]
    public uint new_end_byte;

    public TSPoint start_point;

    public TSPoint old_end_point;

    public TSPoint new_end_point;
}
