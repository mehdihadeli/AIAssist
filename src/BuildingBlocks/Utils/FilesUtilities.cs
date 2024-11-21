using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileSystemGlobbing;

namespace BuildingBlocks.Utils;

public static class FilesUtilities
{
    public static bool IsDirectory(string path)
    {
        return Directory.Exists(path);
    }

    public static bool IsFile(string path)
    {
        return File.Exists(path);
    }

    /// <summary>
    /// Renders an embedded template resource by replacing placeholders with specified replacement values.
    /// </summary>
    /// <param name="assembly">The assembly that contains the embedded template resource.</param>
    /// <param name="fullResourceName">The full name of the embedded resource, including the namespace and any folders, following the pattern:
    /// <c>[DefaultNamespace].[Folder].[Subfolder].[FileName]</c>.</param>
    /// <param name="replacements">An optional object containing key-value pairs for placeholders in the template.</param>
    /// <returns>The rendered template content with placeholders replaced by the specified values, or the original template content if no replacements are provided.</returns>
    public static string ReadEmbeddedResource(Assembly assembly, string fullResourceName, object? replacements = null)
    {
        using Stream? stream = assembly.GetManifestResourceStream(fullResourceName);

        if (stream == null)
        {
            throw new FileNotFoundException($"Resource {fullResourceName} not found.");
        }

        using StreamReader reader = new StreamReader(stream);

        var content = reader.ReadToEnd();

        if (replacements == null)
            return content;

        string processedTemplate = ReplacePlaceholders(content, replacements);

        return processedTemplate;
    }

    public static string NormalizePath(this string path)
    {
        return path.Replace("\\", "/", StringComparison.Ordinal);
    }

    public static bool MatchPattern(string path, IList<string> ignorePatterns)
    {
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing
        var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
        foreach (var pattern in ignorePatterns)
        {
            matcher.AddInclude(pattern);
        }

        var match = matcher.Match(new[] { path }).HasMatches;

        return match;
    }

    public static bool IsExecutableFile(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            return false;
        }

        string extension = Path.GetExtension(path).ToLower(CultureInfo.InvariantCulture);

        // Check file extensions for common executable types
        if (
            extension == ".exe"
            || extension == ".so"
            || extension == ".dll"
            || extension == ".bin"
            || extension == ".apk"
            || extension == ".app"
            || extension == ".msi"
            || extension == ".jar"
        )
        {
            return true;
        }

        return false;
    }

    public static IList<string> LoadIgnorePattersCurrentDirectory(IList<string> fileNames)
    {
        var ignorePatterns = new List<string>();
        var currentWorkingDir = Directory.GetCurrentDirectory();

        foreach (var fileName in fileNames)
        {
            // Recursively load all .gitignore files from working directory and all of its levels
            var gitignoreFiles = Directory.EnumerateFiles(currentWorkingDir, fileName, SearchOption.AllDirectories);
            foreach (var gitignoreFile in gitignoreFiles)
            {
                var patterns = File.ReadAllLines(gitignoreFile)
                    .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                    .ToList();

                ignorePatterns.AddRange(patterns);
            }
        }

        return ignorePatterns.Distinct().ToList();
    }

    public static IList<string> LoadEmbeddedResourceIgnorePatterns(string fullResourceName, Assembly assembly)
    {
        var ignorePatterns = new List<string>();

        //load git ignore from embedded resource
        var embeddedResourceGitIgnorePatterns = ReadEmbeddedResourceIgnore(fullResourceName, assembly);
        ignorePatterns.AddRange(embeddedResourceGitIgnorePatterns);

        return ignorePatterns.Distinct().ToList();
    }

    private static IList<string> ReadEmbeddedResourceIgnore(string fullResourceName, Assembly assembly)
    {
        var content = ReadEmbeddedResource(assembly, fullResourceName);

        var patterns = content
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
            .ToList();

        return patterns;
    }

    private static string ReplacePlaceholders(string template, object replacements)
    {
        // Use reflection to get properties of the anonymous type
        var replacementProperties = replacements.GetType().GetProperties();

        return Regex.Replace(
            template,
            @"{{\s*(\w+)\s*}}",
            match =>
            {
                var key = match.Groups[1].Value.ToLowerInvariant();

                // Find the property that matches the key
                foreach (var prop in replacementProperties)
                {
                    if (prop.Name.ToLowerInvariant().Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        return prop.GetValue(replacements)?.ToString() ?? match.Value;
                    }
                }

                // If no matching property is found, return the original placeholder
                return match.Value;
            }
        );
    }
}
