using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using BuildingBlocks.Types;
using BuildingBlocks.Utils;
using Humanizer;

namespace TreeSitter.Bindings.Queries;

public class QueryManager
{
    // A thread-safe cache to store previously fetched queries
    private static readonly ConcurrentDictionary<string, string> _queryCache = new();

    /// <summary>
    /// Retrieves the default query for the specified programming language, using a cache to improve performance.
    /// </summary>
    /// <param name="language">The programming language for which to get the default query.</param>
    /// <returns>The default query string for the specified programming language.</returns>
    public static string GetDefaultLanguageQuery(ProgrammingLanguage language)
    {
        var languageName = language.Humanize().Dehumanize().ToLower(CultureInfo.InvariantCulture);
        var cacheKey = $"{nameof(QueryConstants.DefaultQueries)}.{languageName}";

        return _queryCache.GetOrAdd(
            cacheKey,
            _ =>
            {
                var assembly = Assembly.GetExecutingAssembly();
                var templateName = $"{languageName}.scm";
                var fullResourceName =
                    $"{nameof(TreeSitter)}.{nameof(Bindings)}.{QueryConstants.DefaultQueries}.{templateName}";

                // Render the embedded template
                string scmQueryContentTemplate = FilesUtilities.ReadEmbeddedResource(assembly, fullResourceName);

                return scmQueryContentTemplate;
            }
        );
    }
}
