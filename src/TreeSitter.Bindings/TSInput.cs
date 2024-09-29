namespace TreeSitter.Bindings;

public unsafe partial struct TSInput
{
    public void* payload;

    [NativeTypeName("const char *(*)(void *, uint32_t, TSPoint, uint32_t *)")]
    public delegate* unmanaged[Cdecl]<void*, uint, TSPoint, uint*, sbyte*> read;

    public TSInputEncoding encoding;
}
