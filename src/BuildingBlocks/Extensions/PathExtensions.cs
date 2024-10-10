using BuildingBlocks.Types;

namespace BuildingBlocks.Extensions;

public static class PathExtensions
{
    // Complete dictionary mapping file extensions to Markdown code block languages
    private static readonly Dictionary<string, string?> _extensionToLanguageMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Programming Languages
            { ".cs", "csharp" },
            { ".js", "javascript" },
            { ".ts", "typescript" },
            { ".json", "json" },
            { ".py", "python" },
            { ".java", "java" },
            { ".cpp", "cpp" },
            { ".c", "c" },
            { ".h", "cpp" },
            { ".hpp", "cpp" },
            { ".rb", "ruby" },
            { ".php", "php" },
            { ".go", "go" },
            { ".rs", "rust" },
            { ".swift", "swift" },
            { ".kt", "kotlin" },
            { ".m", "objective-c" },
            { ".r", "r" },
            { ".pl", "perl" },
            { ".lua", "lua" },
            { ".vb", "vbnet" },
            { ".fs", "fsharp" },
            { ".hs", "haskell" },
            { ".ex", "elixir" },
            { ".erl", "erlang" },
            { ".ml", "ocaml" },
            { ".clj", "clojure" },
            { ".groovy", "groovy" },
            { ".scala", "scala" },
            { ".jl", "julia" },
            // Shell and Scripting
            { ".sh", "bash" },
            { ".zsh", "bash" },
            { ".bash", "bash" },
            { ".ps1", "powershell" },
            { ".bat", "batch" },
            // Web and Markup Languages
            { ".html", "html" },
            { ".htm", "html" },
            { ".css", "css" },
            { ".scss", "scss" },
            { ".sass", "sass" },
            { ".less", "less" },
            { ".xml", "xml" },
            { ".xhtml", "xml" },
            { ".svg", "xml" },
            { ".md", "markdown" },
            { ".markdown", "markdown" },
            // Data Formats
            { ".yml", "yaml" },
            { ".yaml", "yaml" },
            { ".toml", "toml" },
            { ".ini", "ini" },
            { ".csv", "csv" },
            { ".tsv", "csv" },
            { ".sql", "sql" },
            { ".db", "sql" },
            { ".sqlite", "sql" },
            { ".rdf", "xml" },
            { ".rss", "xml" },
            // DevOps Related Extensions
            { ".tf", "hcl" }, // Terraform
            { ".tfvars", "hcl" }, // Terraform variables
            { ".dockerignore", "docker" }, // Docker ignore file
            { "Dockerfile", "docker" }, // Dockerfile
            { "Jenkinsfile", "groovy" }, // Jenkins pipeline file
            { ".gitlab-ci.yml", "yaml" }, // GitLab CI
            { "Vagrantfile", "ruby" }, // Vagrant
            { ".circleci/config.yml", "yaml" }, // CircleCI config
            { ".travis.yml", "yaml" }, // Travis CI
            { "azure-pipelines.yml", "yaml" }, // Azure Pipelines
            { ".tpl", "helm" }, // Helm template file
            // Configuration and Build Files
            { ".dockerfile", "docker" },
            { "makefile", "makefile" },
            { ".makefile", "makefile" },
            { ".cmake", "cmake" },
            { ".gradle", "groovy" },
            { ".csproj", "xml" },
            { ".sln", "plaintext" },
            { ".xcodeproj", "plaintext" },
            { ".project", "xml" },
            // Miscellaneous
            { ".txt", "plaintext" },
            { ".log", "plaintext" },
            { ".conf", "plaintext" },
            { ".cfg", "plaintext" },
            { ".env", "bash" },
        };

    /// <summary>
    /// Get md related language for the file name. for example for test.cs: csharp and test.ts: ts
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string? GetMdLanguageFromFilePath(this string filePath)
    {
        string extension = Path.GetExtension(filePath);

        // Check if the extension exists in our dictionary
        if (_extensionToLanguageMap.TryGetValue(extension, out string? language))
        {
            return language;
        }

        // Handle special cases where files have no extension but are well-known (e.g., Dockerfile, Makefile)
        string fileName = Path.GetFileName(filePath);
        if (_extensionToLanguageMap.TryGetValue(fileName, out language))
        {
            return language;
        }

        // If no mapping found, return "plaintext" as the default
        return string.Empty;
    }

    public static ProgrammingLanguage? GetLanguageFromFilePath(this string filePath)
    {
        // Extract the file extension from the provided file path
        string extension = Path.GetExtension(filePath).ToLowerInvariant();

        // Map file extensions to ProgrammingLanguage enum values
        return extension switch
        {
            ".go" => ProgrammingLanguage.Go,
            ".cs" => ProgrammingLanguage.Csharp,
            ".java" => ProgrammingLanguage.Java,
            ".js" => ProgrammingLanguage.Javascript,
            ".py" => ProgrammingLanguage.Python,
            ".ts" => ProgrammingLanguage.Typescript,
            _ => null,
        };
    }
}
