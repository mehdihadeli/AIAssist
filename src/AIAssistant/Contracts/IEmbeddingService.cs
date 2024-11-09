using AIAssistant.Dtos;
using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssistant.Contracts;

public interface IEmbeddingService
{
    Task<AddEmbeddingsForFilesResult> AddEmbeddingsForFiles(IEnumerable<CodeFileMap> codeFilesMap);
    Task<GetRelatedEmbeddingsResult> GetRelatedEmbeddings(string userQuery);
    Task<GetEmbeddingResult> GenerateEmbeddingForUserInput(string userInput);
}
