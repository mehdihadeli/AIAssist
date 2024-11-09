namespace BuildingBlocks.InMemoryVectorDatabase.Contracts;

public interface IVectorContext
{
    IVectorCollection<T> GetCollection<T>(string name)
        where T : Document, new();

    void AddOrUpdateDocument<T>(string collectionName, T document)
        where T : Document, new();

    void AddOrUpdateDocuments<T>(string collectionName, IEnumerable<T> documents)
        where T : Document, new();

    IEnumerable<T> QueryByDocumentFilter<T>(
        string collectionName,
        Func<T, bool>? documentFilter = null,
        IDictionary<string, string>? metadataFilter = null
    )
        where T : Document, new();

    IEnumerable<T> QueryDocumentsByEmbedding<T>(
        string collectionName,
        IList<double> queryEmbedding,
        IDictionary<string, string>? metadataFilter = null,
        int nResults = 0,
        decimal threshold = 0.3m
    )
        where T : Document, new();
}
