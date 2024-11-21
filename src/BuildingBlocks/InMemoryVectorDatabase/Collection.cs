using BuildingBlocks.InMemoryVectorDatabase.Contracts;

namespace BuildingBlocks.InMemoryVectorDatabase;

public class VectorCollection<T>(string name) : IVectorCollection<T>
    where T : Document
{
    private readonly Dictionary<Guid, T> _documents = new();

    public string Name { get; private set; } = name;

    public void AddOrUpdateDocument(T document)
    {
        ArgumentException.ThrowIfNullOrEmpty(document.Text);

        _documents[document.Id] = document;
    }

    public void AddOrUpdateDocuments(IEnumerable<T> documents)
    {
        foreach (var document in documents)
        {
            AddOrUpdateDocument(document);
        }
    }

    public IEnumerable<T> QueryByFilter(IDictionary<string, string> metadataFilter)
    {
        return _documents.Values.Where(doc => MetadataMatches(doc.Metadata, metadataFilter)).ToList();
    }

    public IEnumerable<T> QueryByDocumentFilter(
        Func<T, bool>? documentFilter = null,
        IDictionary<string, string>? metadataFilter = null
    )
    {
        return _documents
            .Values.Where(doc =>
                (metadataFilter == null || MetadataMatches(doc.Metadata, metadataFilter))
                && (documentFilter == null || documentFilter(doc))
            )
            .ToList();
    }

    public IEnumerable<T> QueryDocuments(
        IList<double> queryEmbedding,
        IDictionary<string, string>? metadataFilter = null,
        int nResults = 0,
        decimal threshold = 0.3m
    )
    {
        var query = _documents
            .Values.Select(doc => new
            {
                Document = doc,
                SimilarityScore = CosineSimilarity(doc.Embeddings, queryEmbedding),
            })
            .Where(doc => metadataFilter == null || MetadataMatches(doc.Document.Metadata, metadataFilter));

        query = query
            .Where(x => x.SimilarityScore >= threshold)
            .OrderByDescending(result => result.SimilarityScore)
            .ToList();

        if (!query.Any())
        {
            throw new Exception(
                $"The current threshold for your embedding model config is {threshold}, you can try with lower threshold."
            );
        }

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

    private decimal CosineSimilarity(IList<double> vec1, IList<double> vec2)
    {
        var dotProduct = vec1.Zip(vec2, (a, b) => a * b).Sum();
        var magnitude1 = Math.Sqrt(vec1.Sum(v => v * v));
        var magnitude2 = Math.Sqrt(vec2.Sum(v => v * v));

        if (magnitude1 == 0 || magnitude2 == 0)
            return 0.0m;

        var result = (dotProduct / (magnitude1 * magnitude2));

        return Convert.ToDecimal(result);
    }
}
