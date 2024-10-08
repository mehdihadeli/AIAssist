using AIAssistant.Models;
using BuildingBlocks.Extensions;
using BuildingBlocks.InMemoryVectorDatabase;
using Humanizer;

namespace AIAssistant.Data;

public class EmbeddingsStore(VectorDatabase vectorDatabase)
{
    //private readonly List<CodeEmbedding> _codeEmbeddings = new();
    private readonly Collection _collection = vectorDatabase.CreateOrGetCollection("code-embeddings");

    public Task AddCodeEmbeddings(IEnumerable<CodeEmbedding> codeEmbeddings)
    {
        foreach (var codeEmbedding in codeEmbeddings)
        {
            IDictionary<string, string> metadata = new
            {
                sessionId = codeEmbedding.SessionId,
                relativeFilePath = codeEmbedding.RelativeFilePath,
                id = codeEmbedding.Id,
                treeSitterCode = codeEmbedding.TreeSitterCode,
            }.AnonymouseTypeToDictionary();

            _collection.AddDocuments(codeEmbedding.Code, codeEmbedding.Embeddings, codeEmbedding.Id, metadata);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Find most similarity items based on embedding inputs and stored embedding data
    /// </summary>
    /// <param name="userEmbeddingQuery"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public IEnumerable<CodeEmbedding> Query(IList<double> userEmbeddingQuery, Guid sessionId)
    {
        // https://ollama.com/blog/embedding-models
        // https://github.com/chroma-core/chroma
        var res = _collection
            .QueryDocuments(
                userEmbeddingQuery,
                new Dictionary<string, string> { { nameof(sessionId), sessionId.ToString() } }
            )
            .Select(x => new CodeEmbedding
            {
                RelativeFilePath = x.Metadata[nameof(CodeEmbedding.RelativeFilePath).Camelize()],
                Embeddings = x.Embeddings,
                Code = x.Text,
                SessionId = Guid.Parse(x.Metadata[nameof(CodeEmbedding.SessionId).Camelize()]),
                Id = Guid.Parse(x.Metadata[nameof(CodeEmbedding.Id).Camelize()]),
                TreeSitterCode = x.Metadata[nameof(CodeEmbedding.TreeSitterCode).Camelize()],
            });

        return res.ToList().AsReadOnly();
    }

    public Task ClearEmbeddingsBySession(Guid sessionId)
    {
        // _codeEmbeddings.RemoveAll(x => x.SessionId == sessionId);

        return Task.CompletedTask;
    }
}
