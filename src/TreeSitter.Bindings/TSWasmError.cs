namespace TreeSitter.Bindings;

public unsafe partial struct TSWasmError
{
    public TSWasmErrorKind kind;

    [NativeTypeName("char *")]
    public sbyte* message;
}
