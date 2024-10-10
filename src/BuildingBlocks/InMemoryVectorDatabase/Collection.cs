using BuildingBlocks.LLM;

namespace BuildingBlocks.InMemoryVectorDatabase;

public class Collection(string name)
{
    private readonly Dictionary<Guid, Document> _documents = new();

    public string Name { get; private set; } = name;

    // Add documents to the collection
    public void AddDocuments(string text, IList<double> embeddings, Guid id, IDictionary<string, string> metadata)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);

        var document = new Document
        {
            Id = id,
            Text = text,
            Metadata = metadata,
            Embeddings = embeddings,
        };
        _documents[id] = document;
    }

    /// <summary>
    /// Query documents by text similarity with optional metadata filter
    /// </summary>
    /// <param name="queryText"></param>
    /// <param name="nResults"></param>
    /// <param name="metadataFilter"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    public IEnumerable<Document> QueryDocuments(
        string queryText,
        IDictionary<string, string>? metadataFilter = null,
        int nResults = 0,
        double threshold = 0.3
    )
    {
        var queryEmbedding = TokenizerHelper.GPT4VectorTokens(queryText);

        var query = _documents
            .Values.Select(doc => new
            {
                Document = doc,
                SimilarityScore = CosineSimilarity(doc.Embeddings, queryEmbedding, doc),
            })
            .Where(doc => metadataFilter == null || MetadataMatches(doc.Document.Metadata, metadataFilter))
            .Where(x => x.SimilarityScore >= threshold)
            .OrderByDescending(result => result.SimilarityScore)
            .ToList();

        var averageRank = query.Select(x => x.SimilarityScore).Average();

        var result =
            nResults > 0
                ? query.Where(x => x.SimilarityScore >= averageRank).Take(nResults).Select(result => result.Document)
                : query.Where(x => x.SimilarityScore >= averageRank).Select(result => result.Document);

        return result;
    }

    /// <summary>
    /// Query documents by text similarity with optional metadata filter
    /// </summary>
    /// <param name="queryEmbedding"></param>
    /// <param name="nResults"></param>
    /// <param name="metadataFilter"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    public IEnumerable<Document> QueryDocuments(
        IList<double> queryEmbedding,
        IDictionary<string, string>? metadataFilter = null,
        int nResults = 0,
        double threshold = 0.3
    )
    {
        var query = _documents
            .Values.Select(doc => new
            {
                Document = doc,
                SimilarityScore = CosineSimilarity(doc.Embeddings, queryEmbedding, doc),
            })
            .Where(doc => metadataFilter == null || MetadataMatches(doc.Document.Metadata, metadataFilter))
            .Where(x => x.SimilarityScore >= threshold)
            .OrderByDescending(result => result.SimilarityScore)
            .ToList();

        var averageRank = query.Select(x => x.SimilarityScore).Average();

        var result =
            nResults > 0
                ? query.Where(x => x.SimilarityScore >= averageRank).Take(nResults).Select(result => result.Document)
                : query.Where(x => x.SimilarityScore >= averageRank).Select(result => result.Document);

        return result;
    }

    // Check if the document metadata matches the given filter
    private bool MetadataMatches(IDictionary<string, string> documentMetadata, IDictionary<string, string>? filter)
    {
        if (filter == null)
        {
            return true; // No filtering applied, so all documents match.
        }

        return filter.All(kvp => documentMetadata.ContainsKey(kvp.Key) && documentMetadata[kvp.Key] == kvp.Value);
    }

    private double CosineSimilarity(IList<double> vec1, IList<double> vec2, Document document)
    {
        var dotProduct = vec1.Zip(vec2, (a, b) => a * b).Sum();
        var magnitude1 = Math.Sqrt(vec1.Sum(v => v * v));
        var magnitude2 = Math.Sqrt(vec2.Sum(v => v * v));

        if (magnitude1 == 0 || magnitude2 == 0)
            return 0.0;

        var result = dotProduct / (magnitude1 * magnitude2);

        return result;
    }
}
