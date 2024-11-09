namespace BuildingBlocks.InMemoryVectorDatabase.Contracts;

public interface IVectorCollection<T>
    where T : Document
{
    string Name { get; }
    void AddOrUpdateDocument(T document);
    void AddOrUpdateDocuments(IEnumerable<T> documents);
    IEnumerable<T> QueryByFilter(IDictionary<string, string> metadataFilter);

    IEnumerable<T> QueryByDocumentFilter(
        Func<T, bool>? documentFilter = null,
        IDictionary<string, string>? metadataFilter = null
    );

    IEnumerable<T> QueryDocuments(
        IList<double> queryEmbedding,
        IDictionary<string, string>? metadataFilter = null,
        int nResults = 0,
        decimal threshold = 0.3m
    );
}
