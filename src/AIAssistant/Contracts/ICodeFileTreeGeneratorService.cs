using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssistant.Contracts;

public interface ICodeFileTreeGeneratorService
{
    IEnumerable<CodeFileMap> GetOrAddCodeTreeMapFromFiles(string contextWorkingDir, IList<string>? extraFiles = null);
    IEnumerable<CodeFileMap> AddOrUpdateCodeTreeMapFromFiles(IList<string>? files);
}
