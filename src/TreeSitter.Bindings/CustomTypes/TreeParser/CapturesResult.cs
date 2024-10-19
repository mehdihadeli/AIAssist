namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class CapturesResult
{
    public IList<DefinitionCaptureItem> DefinitionCaptureItems { get; } = new List<DefinitionCaptureItem>();

    // We exclude references because most of the references can be found in our definitions like functions
    public IList<ReferenceCaptureItem> ReferenceCaptureItems { get; } = new List<ReferenceCaptureItem>();
}
