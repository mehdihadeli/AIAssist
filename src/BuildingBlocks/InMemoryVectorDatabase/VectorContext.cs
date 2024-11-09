using BuildingBlocks.InMemoryVectorDatabase.Contracts;

namespace BuildingBlocks.InMemoryVectorDatabase;

public class VectorContext : IVectorContext
{
    private readonly Dictionary<string, object> _collections = new();

    /// <summary>
    /// Gets or creates a collection of a specified type T by name
    /// </summary>
    /// <param name="name"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IVectorCollection<T> GetCollection<T>(string name)
        where T : Document, new()
    {
        if (_collections.TryGetValue(name, out var collection))
        {
            return (IVectorCollection<T>)collection;
        }

        var newCollection = new VectorCollection<T>(name);
        _collections[name] = newCollection;
        return newCollection;
    }

    /// <summary>
    /// Adds or updates a document in a specified collection
    /// </summary>
    /// <param name="collectionName"></param>
    /// <param name="document"></param>
    /// <typeparam name="T"></typeparam>
    public void AddOrUpdateDocument<T>(string collectionName, T document)
        where T : Document, new()
    {
        var collection = GetCollection<T>(collectionName);
        collection.AddOrUpdateDocument(document);
    }

    /// <summary>
    /// Adds or updates multiple documents in a specified collection
    /// </summary>
    /// <param name="collectionName"></param>
    /// <param name="documents"></param>
    /// <typeparam name="T"></typeparam>
    public void AddOrUpdateDocuments<T>(string collectionName, IEnumerable<T> documents)
        where T : Document, new()
    {
        var collection = GetCollection<T>(collectionName);
        collection.AddOrUpdateDocuments(documents);
    }

    /// <summary>
    /// Queries a collection by filter on metadata or document properties
    /// </summary>
    /// <param name="collectionName"></param>
    /// <param name="documentFilter"></param>
    /// <param name="metadataFilter"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IEnumerable<T> QueryByDocumentFilter<T>(
        string collectionName,
        Func<T, bool>? documentFilter = null,
        IDictionary<string, string>? metadataFilter = null
    )
        where T : Document, new()
    {
        var collection = GetCollection<T>(collectionName);
        return collection.QueryByDocumentFilter(documentFilter, metadataFilter);
    }

    /// <summary>
    /// Queries a collection by embedding similarity
    /// </summary>
    /// <param name="collectionName"></param>
    /// <param name="queryEmbedding"></param>
    /// <param name="metadataFilter"></param>
    /// <param name="nResults"></param>
    /// <param name="threshold"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IEnumerable<T> QueryDocumentsByEmbedding<T>(
        string collectionName,
        IList<double> queryEmbedding,
        IDictionary<string, string>? metadataFilter = null,
        int nResults = 0,
        decimal threshold = 0.3m
    )
        where T : Document, new()
    {
        var collection = GetCollection<T>(collectionName);
        return collection.QueryDocuments(queryEmbedding, metadataFilter, nResults, threshold);
    }
}
