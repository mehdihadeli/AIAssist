namespace TreeSitter.Bindings;

public unsafe partial struct TSLogger
{
    public void* payload;

    [NativeTypeName("void (*)(void *, TSLogType, const char *)")]
    public delegate* unmanaged[Cdecl]<void*, TSLogType, sbyte*, void> log;
}
