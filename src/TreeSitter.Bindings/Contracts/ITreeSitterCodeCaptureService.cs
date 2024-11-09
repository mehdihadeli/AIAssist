using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace TreeSitter.Bindings.Contracts;

public interface ITreeSitterCodeCaptureService
{
    IReadOnlyList<DefinitionCaptureItem> CreateTreeSitterMap(IEnumerable<CodeFile> codeFiles);
}
