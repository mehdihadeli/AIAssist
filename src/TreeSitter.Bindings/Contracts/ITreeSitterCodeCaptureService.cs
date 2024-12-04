using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace TreeSitter.Bindings.Contracts;

public interface ITreeSitterCodeCaptureService
{
    IReadOnlyList<DefinitionCapture> CreateTreeSitterMap(IEnumerable<CodeFile> codeFiles);
}
