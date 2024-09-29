namespace TreeSitter.Bindings;

public enum TSWasmErrorKind
{
    TSWasmErrorKindNone = 0,
    TSWasmErrorKindParse,
    TSWasmErrorKindCompile,
    TSWasmErrorKindInstantiate,
    TSWasmErrorKindAllocate,
}
