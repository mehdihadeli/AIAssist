using BuildingBlocks.Types;
using TreeSitter.Bindings.CustomTypes;

namespace TreeSitter.Bindings.Contracts;

public interface ITreeSitterParser
{
    unsafe string GetRootNodeExpression(ProgrammingLanguage language, string code);
    unsafe TSNode GetRootNode(ProgrammingLanguage language, string code);
    unsafe TSNode GetRootNode(TSTree* tree);
    unsafe TSParser* GetParser(ProgrammingLanguage language);
    unsafe TSTree* GetCodeTree(TSParser* parser, string code);
    unsafe TSLanguage* GetLanguage(ProgrammingLanguage programmingLanguage);
    unsafe TSQuery* GetLanguageDefaultQuery(ProgrammingLanguage programmingLanguage);
}
