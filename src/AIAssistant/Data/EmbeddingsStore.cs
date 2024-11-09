using AIAssistant.Models;
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
            IDictionary<string, string> metadata = new Dictionary<string, string>
            {
                { nameof(CodeEmbedding.SessionId).Camelize(), codeEmbedding.SessionId.ToString() },
                { nameof(CodeEmbedding.RelativeFilePath).Camelize(), codeEmbedding.RelativeFilePath },
                { nameof(CodeEmbedding.Id).Camelize(), codeEmbedding.Id.ToString() },
                { nameof(CodeEmbedding.TreeSitterFullCode).Camelize(), codeEmbedding.TreeSitterFullCode },
                { nameof(CodeEmbedding.TreeOriginalCode).Camelize(), codeEmbedding.TreeOriginalCode },
            };

            _collection.AddDocuments(codeEmbedding.Code, codeEmbedding.Embeddings, codeEmbedding.Id, metadata);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Find most similarity items based on embedding inputs and stored embedding data
    /// </summary>
    /// <param name="userEmbeddingQuery"></param>
    /// <param name="sessionId"></param>
    /// <param name="threshold"></param>
    /// <param name="nResults"></param>
    /// <returns></returns>
    public IEnumerable<CodeEmbedding> Query(
        IList<double> userEmbeddingQuery,
        Guid sessionId,
        decimal threshold,
        int nResults = 0
    )
    {
        // https://ollama.com/blog/embedding-models
        // https://github.com/chroma-core/chroma
        var res = _collection
            .QueryDocuments(
                userEmbeddingQuery,
                new Dictionary<string, string> { { nameof(sessionId), sessionId.ToString() } },
                nResults: nResults,
                threshold: threshold
            )
            .Select(x => new CodeEmbedding
            {
                RelativeFilePath = x.Metadata[nameof(CodeEmbedding.RelativeFilePath).Camelize()],
                Embeddings = x.Embeddings,
                Code = x.Text,
                SessionId = Guid.Parse(x.Metadata[nameof(CodeEmbedding.SessionId).Camelize()]),
                Id = Guid.Parse(x.Metadata[nameof(CodeEmbedding.Id).Camelize()]),
                TreeSitterFullCode = x.Metadata[nameof(CodeEmbedding.TreeSitterFullCode).Camelize()],
                TreeOriginalCode = x.Metadata[nameof(CodeEmbedding.TreeOriginalCode).Camelize()],
            });

        return res.ToList().AsReadOnly();
    }

    public Task ClearEmbeddingsBySession(Guid sessionId)
    {
        // _codeEmbeddings.RemoveAll(x => x.SessionId == sessionId);

        return Task.CompletedTask;
    }
}
