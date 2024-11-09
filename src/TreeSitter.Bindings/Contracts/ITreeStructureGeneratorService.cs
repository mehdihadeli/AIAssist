using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace TreeSitter.Bindings.Contracts;

public interface ITreeStructureGeneratorService
{
    string GenerateOriginalCodeTree(string originalCode, string relativePath);
    string GenerateTreeSitter(IList<DefinitionCaptureItem> definitionItems, bool isFull);
}
