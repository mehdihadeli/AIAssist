namespace TreeSitter.Bindings;

public unsafe partial struct TSLexer
{
    [NativeTypeName("int32_t")]
    public int lookahead;

    [NativeTypeName("TSSymbol")]
    public ushort result_symbol;

    [NativeTypeName("void (*)(TSLexer *, bool)")]
    public delegate* unmanaged[Cdecl]<TSLexer*, bool, void> advance;

    [NativeTypeName("void (*)(TSLexer *)")]
    public delegate* unmanaged[Cdecl]<TSLexer*, void> mark_end;

    [NativeTypeName("uint32_t (*)(TSLexer *)")]
    public delegate* unmanaged[Cdecl]<TSLexer*, uint> get_column;

    [NativeTypeName("bool (*)(const TSLexer *)")]
    public delegate* unmanaged[Cdecl]<TSLexer*, bool> is_at_included_range_start;

    [NativeTypeName("bool (*)(const TSLexer *)")]
    public delegate* unmanaged[Cdecl]<TSLexer*, bool> eof;

    [NativeTypeName("void (*)(const TSLexer *, const char *, ...)")]
    public delegate* unmanaged[Cdecl]<TSLexer*, sbyte*, void> log;
}
