using AIAssistant.Chat.Models;
using AIAssistant.Data;
using AIAssistant.Dtos;
using AIAssistant.Models;
using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssistant.Contracts;

public interface IEmbeddingService
{
    Task<AddEmbeddingsForFilesResult> AddOrUpdateEmbeddingsForFiles(
        IEnumerable<CodeFileMap> codeFilesMap,
        ChatSession chatSession
    );
    Task<GetRelatedEmbeddingsResult> GetRelatedEmbeddings(string userQuery, ChatSession chatSession);
    public IEnumerable<CodeEmbedding> QueryByFilter(
        ChatSession chatSession,
        Func<CodeEmbeddingDocument, bool>? documentFilter = null,
        IDictionary<string, string>? metadataFilter = null
    );
    Task<GetEmbeddingResult> GenerateEmbeddingForUserInput(string userInput);
}
