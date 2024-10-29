using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssistant.Contracts;

public interface ICodeFileMapService
{
    IEnumerable<CodeFileMap> GenerateCodeFileMaps(IReadOnlyList<DefinitionCaptureItem> definitions);
}
