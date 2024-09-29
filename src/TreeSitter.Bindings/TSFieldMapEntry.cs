namespace TreeSitter.Bindings;

public partial struct TSFieldMapEntry
{
    [NativeTypeName("TSFieldId")]
    public ushort field_id;

    [NativeTypeName("uint8_t")]
    public byte child_index;

    public bool inherited;
}
