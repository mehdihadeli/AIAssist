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
            QueryConstants.DefaultQueries,
            $"{languageName}.scm",
            null
        );

        return scmQueryContent;
    }

    public static string GetSimpleLanguageQuery(ProgrammingLanguage language)
    {
        var languageName = language.Humanize().Dehumanize().ToLower(CultureInfo.InvariantCulture);

        string scmQueryContent = FilesUtilities.RenderTemplate(
            QueryConstants.SimpleQueries,
            $"{languageName}.scm",
            null
        );

        return scmQueryContent;
    }
}
