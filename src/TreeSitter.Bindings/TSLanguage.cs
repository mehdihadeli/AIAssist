namespace TreeSitter.Bindings;

public unsafe partial struct TSLanguage
{
    [NativeTypeName("uint32_t")]
    public uint version;

    [NativeTypeName("uint32_t")]
    public uint symbol_count;

    [NativeTypeName("uint32_t")]
    public uint alias_count;

    [NativeTypeName("uint32_t")]
    public uint token_count;

    [NativeTypeName("uint32_t")]
    public uint external_token_count;

    [NativeTypeName("uint32_t")]
    public uint state_count;

    [NativeTypeName("uint32_t")]
    public uint large_state_count;

    [NativeTypeName("uint32_t")]
    public uint production_id_count;

    [NativeTypeName("uint32_t")]
    public uint field_count;

    [NativeTypeName("uint16_t")]
    public ushort max_alias_sequence_length;

    [NativeTypeName("const uint16_t *")]
    public ushort* parse_table;

    [NativeTypeName("const uint16_t *")]
    public ushort* small_parse_table;

    [NativeTypeName("const uint32_t *")]
    public uint* small_parse_table_map;

    [NativeTypeName("const TSParseActionEntry *")]
    public TSParseActionEntry* parse_actions;

    [NativeTypeName("const char *const *")]
    public sbyte** symbol_names;

    [NativeTypeName("const char *const *")]
    public sbyte** field_names;

    [NativeTypeName("const TSFieldMapSlice *")]
    public TSFieldMapSlice* field_map_slices;

    [NativeTypeName("const TSFieldMapEntry *")]
    public TSFieldMapEntry* field_map_entries;

    [NativeTypeName("const TSSymbolMetadata *")]
    public TSSymbolMetadata* symbol_metadata;

    [NativeTypeName("const TSSymbol *")]
    public ushort* public_symbol_map;

    [NativeTypeName("const uint16_t *")]
    public ushort* alias_map;

    [NativeTypeName("const TSSymbol *")]
    public ushort* alias_sequences;

    [NativeTypeName("const TSLexMode *")]
    public TSLexMode* lex_modes;

    [NativeTypeName("bool (*)(TSLexer *, TSStateId)")]
    public delegate* unmanaged[Cdecl]<TSLexer*, ushort, bool> lex_fn;

    [NativeTypeName("bool (*)(TSLexer *, TSStateId)")]
    public delegate* unmanaged[Cdecl]<TSLexer*, ushort, bool> keyword_lex_fn;

    [NativeTypeName("TSSymbol")]
    public ushort keyword_capture_token;

    [NativeTypeName("__AnonymousRecord_parser_L122_C3")]
    public _external_scanner_e__Struct external_scanner;

    [NativeTypeName("const TSStateId *")]
    public ushort* primary_state_ids;

    public unsafe partial struct _external_scanner_e__Struct
    {
        [NativeTypeName("const bool *")]
        public bool* states;

        [NativeTypeName("const TSSymbol *")]
        public ushort* symbol_map;

        [NativeTypeName("void *(*)()")]
        public delegate* unmanaged[Cdecl]<void*> create;

        [NativeTypeName("void (*)(void *)")]
        public delegate* unmanaged[Cdecl]<void*, void> destroy;

        [NativeTypeName("bool (*)(void *, TSLexer *, const bool *)")]
        public delegate* unmanaged[Cdecl]<void*, TSLexer*, bool*, bool> scan;

        [NativeTypeName("unsigned int (*)(void *, char *)")]
        public delegate* unmanaged[Cdecl]<void*, sbyte*, uint> serialize;

        [NativeTypeName("void (*)(void *, const char *, unsigned int)")]
        public delegate* unmanaged[Cdecl]<void*, sbyte*, uint, void> deserialize;
    }
}
