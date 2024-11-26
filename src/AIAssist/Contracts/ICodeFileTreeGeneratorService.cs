using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssist.Contracts;

public interface ICodeFileTreeGeneratorService
{
    IList<CodeFileMap> GetCodeFilesMap(IList<string> files);
    CodeFileMap? GetCodeFileMap(string file);
    IList<CodeFileMap> AddContextCodeFilesMap(IList<string> files);
    IList<CodeFileMap> AddOrUpdateCodeFilesMap(IList<string> files);
    CodeFileMap? AddOrUpdateCodeFileMap(string file);
}
