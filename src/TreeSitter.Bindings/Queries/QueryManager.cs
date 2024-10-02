using Humanizer;
using TreeSitter.Bindings.Utilities;

namespace TreeSitter.Bindings.Queries;

public static class QueryManager
{
    public static string GetDefaultLanguageQuery(ProgrammingLanguage language)
    {
        var languageName = language.Humanize().Transform(To.LowerCase);
        string scmQueryFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            QueryConstants.QueriesDefault,
            $"{languageName}.scm"
        );

        string scmQueryContent = File.ReadAllText(scmQueryFilePath);

        return scmQueryContent;
    }
}
