namespace TreeSitter.Bindings;

public partial struct TSQueryPredicateStep
{
    public TSQueryPredicateStepType type;

    [NativeTypeName("uint32_t")]
    public uint value_id;
}
