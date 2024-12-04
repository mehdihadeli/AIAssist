using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace TreeSitter.Bindings.Contracts;

public interface ITreeStructureGeneratorService
{
    /// <summary>
    /// Tree for original code.
    /// </summary>
    /// <param name="originalCode"></param>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    string GenerateOriginalCodeTree(string originalCode, string relativePath);
    string GenerateTreeSitter(IList<DefinitionCapture> definitionCaptures, bool isFull);
}
