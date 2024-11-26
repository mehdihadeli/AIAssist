using AIAssist.Chat.Models;
using AIAssist.Data;
using AIAssist.Dtos;
using AIAssist.Models;
using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssist.Contracts;

public interface IEmbeddingService
{
    Task<AddEmbeddingsForFilesResult> AddOrUpdateEmbeddingsForFiles(
        IList<CodeFileMap> codeFilesMap,
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
