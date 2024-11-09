using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssistant.Contracts;

public interface ICodeLoaderService
{
    IReadOnlyList<DefinitionCaptureItem> LoadTreeSitterCodeCaptures(
        string contextWorkingDir,
        IList<string>? extraFiles = null
    );
}
