namespace TreeSitter.Bindings;

public partial struct TSCharacterRange
{
    [NativeTypeName("int32_t")]
    public int start;

    [NativeTypeName("int32_t")]
    public int end;
}
