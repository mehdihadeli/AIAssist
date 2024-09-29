using System.Runtime.InteropServices;

namespace TreeSitter.Bindings;

[StructLayout(LayoutKind.Explicit)]
public partial struct TSParseAction
{
    [FieldOffset(0)]
    [NativeTypeName("__AnonymousRecord_parser_L61_C3")]
    public _shift_e__Struct shift;

    [FieldOffset(0)]
    [NativeTypeName("__AnonymousRecord_parser_L67_C3")]
    public _reduce_e__Struct reduce;

    [FieldOffset(0)]
    [NativeTypeName("uint8_t")]
    public byte type;

    public partial struct _shift_e__Struct
    {
        [NativeTypeName("uint8_t")]
        public byte type;

        [NativeTypeName("TSStateId")]
        public ushort state;

        public bool extra;

        public bool repetition;
    }

    public partial struct _reduce_e__Struct
    {
        [NativeTypeName("uint8_t")]
        public byte type;

        [NativeTypeName("uint8_t")]
        public byte child_count;

        [NativeTypeName("TSSymbol")]
        public ushort symbol;

        [NativeTypeName("int16_t")]
        public short dynamic_precedence;

        [NativeTypeName("uint16_t")]
        public ushort production_id;
    }
}
