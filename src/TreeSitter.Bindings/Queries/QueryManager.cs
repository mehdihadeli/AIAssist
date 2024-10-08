using System.Globalization;
using BuildingBlocks.Types;
using BuildingBlocks.Utils;
using Humanizer;

namespace TreeSitter.Bindings.Queries;

public static class QueryManager
{
    public static string GetDefaultLanguageQuery(ProgrammingLanguage language)
    {
        var languageName = language.Humanize().Dehumanize().ToLower(CultureInfo.InvariantCulture);

        string scmQueryContent = FilesUtilities.RenderTemplate(
            QueryConstants.QueriesDefault,
            $"{languageName}.scm",
            null
        );

        return scmQueryContent;
    }
}
