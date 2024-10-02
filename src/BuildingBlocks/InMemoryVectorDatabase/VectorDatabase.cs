namespace BuildingBlocks.InMemoryVectorDatabase;

public class VectorDatabase
{
    private readonly Dictionary<string, Collection> _collections = new();

    /// <summary>
    /// Create or get an existing collection
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Collection CreateOrGetCollection(string name)
    {
        if (!_collections.TryGetValue(name, out Collection? value))
        {
            value = new Collection(name);
            _collections[name] = value;
        }
        return value;
    }

    /// <summary>
    /// List all collections
    /// </summary>
    /// <returns></returns>
    public IList<string> ListCollections()
    {
        return _collections.Keys.ToList();
    }
}
