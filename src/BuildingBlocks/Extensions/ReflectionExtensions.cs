using System.Reflection;

namespace BuildingBlocks.Extensions;

public static class ReflectionExtensions
{
    public static IDictionary<string, string> AnonymouseTypeToDictionary<T>(this T anonymousType)
    {
        var metadata = new Dictionary<string, string>();

        // Use reflection to get the properties of the anonymous type
        PropertyInfo[] properties = typeof(T).GetProperties();

        foreach (var prop in properties)
        {
            string key = prop.Name;
            string value = prop.GetValue(anonymousType)?.ToString();
            metadata.Add(key, value);
        }

        return metadata;
    }
}
