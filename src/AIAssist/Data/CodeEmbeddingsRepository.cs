using AIAssist.Models;
using BuildingBlocks.InMemoryVectorDatabase.Contracts;
using Humanizer;

namespace AIAssist.Data;

public class CodeEmbeddingsRepository(IVectorContext vectorDatabase) : ICodeEmbeddingsRepository
{
    private readonly IVectorCollection<CodeEmbeddingDocument> _codeEmbeddingsCollection =
        vectorDatabase.GetCollection<CodeEmbeddingDocument>("code-embeddings");

    public Task AddOrUpdateCodeEmbeddings(IEnumerable<CodeEmbedding> codeEmbeddings)
    {
        var codeEmbeddingsDocument = codeEmbeddings.Select(codeEmbedding =>
        {
            var metadata = new Dictionary<string, string>
            {
                { nameof(CodeEmbedding.SessionId).Camelize(), codeEmbedding.SessionId.ToString() },
                { nameof(CodeEmbedding.RelativeFilePath).Camelize(), codeEmbedding.RelativeFilePath },
                { nameof(CodeEmbedding.Id).Camelize(), codeEmbedding.Id.ToString() },
                { nameof(CodeEmbedding.TreeSitterFullCode).Camelize(), codeEmbedding.TreeSitterFullCode },
                { nameof(CodeEmbedding.TreeOriginalCode).Camelize(), codeEmbedding.TreeOriginalCode },
            };

            return new CodeEmbeddingDocument
            {
                Id = codeEmbedding.Id,
                Text = codeEmbedding.Code,
                Embeddings = codeEmbedding.Embeddings,
                Metadata = metadata,
            };
        });

        _codeEmbeddingsCollection.AddOrUpdateDocuments(codeEmbeddingsDocument);

        return Task.CompletedTask;
    }

    public IEnumerable<CodeEmbedding> QueryByDocumentFilter(
        Guid sessionId,
        Func<CodeEmbeddingDocument, bool>? documentFilter = null,
        IDictionary<string, string>? metadataFilter = null
    )
    {
        var filteredDocuments = _codeEmbeddingsCollection.QueryByDocumentFilter(
            documentFilter: documentFilter,
            metadataFilter: metadataFilter
        );

        var codeEmbeddings = filteredDocuments.Select(doc => new CodeEmbedding
        {
            Id = doc.Id,
            Code = doc.Text,
            Embeddings = doc.Embeddings,
            SessionId = sessionId,
            RelativeFilePath = doc.Metadata[nameof(CodeEmbedding.RelativeFilePath).Camelize()],
            TreeSitterFullCode = doc.Metadata[nameof(CodeEmbedding.TreeSitterFullCode).Camelize()],
            TreeOriginalCode = doc.Metadata[nameof(CodeEmbedding.TreeOriginalCode).Camelize()],
        });

        return codeEmbeddings;
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
        var res = _codeEmbeddingsCollection
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
