namespace BuildingBlocks.InMemoryVectorDatabase.Contracts;

public interface IGenericVectorRepository<T, out TDocument>
    where T : class
    where TDocument : Document
{
    Task AddOrUpdateCodeEmbeddings(IEnumerable<T> codeEmbeddings);
    IEnumerable<T> Query(IList<double> userEmbeddingQuery, Guid sessionId, decimal threshold, int nResults = 0);
    IEnumerable<T> QueryByDocumentFilter(
        Guid sessionId,
        Func<TDocument, bool>? documentFilter = null,
        IDictionary<string, string>? metadataFilter = null
    );
    Task ClearEmbeddingsBySession(Guid sessionId);
}
