using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileSystemGlobbing;

namespace BuildingBlocks.Utils;

public class FilesUtilities
{
    private static readonly IList<string> _gitignorePatterns;

    static FilesUtilities()
    {
        _gitignorePatterns = LoadGitignorePatterns();
    }

    public static string RenderTemplate(string templateFolder, string templateName, object? replacements)
    {
        string templateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, templateFolder, templateName);

        string templateContent = File.ReadAllText(templateFilePath);

        if (replacements == null)
            return templateContent;

        string processedTemplate = ReplacePlaceholders(templateContent, replacements);

        return processedTemplate;
    }

    public static bool IsIgnored(string path)
    {
        if (IsDirectory(path))
        {
            // Check if the path doesn't end with a directory separator and add it, to detecting directory by regex matcher
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                path += Path.DirectorySeparatorChar; // Add the correct separator for the platform
            }
        }

        var relativeToRootPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), path);

        // Check if the file itself matches any ignore pattern
        if (MatchPattern(relativeToRootPath, _gitignorePatterns))
        {
            return true;
        }

        // Check if it's an executable or a file type that should be ignored
        if (IsExecutableFile(relativeToRootPath))
        {
            return true;
        }

        return false;
    }

    private static bool MatchPattern(string path, IList<string> ignorePatterns)
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

    public static bool IsDirectory(string path)
    {
        return Directory.Exists(path);
    }

    public static bool IsFile(string path)
    {
        return File.Exists(path);
    }

    private static bool IsExecutableFile(string path)
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

    private static IList<string> LoadGitignorePatterns()
    {
        var gitignorePatterns = new List<string>();
        var currentWorkingDir = Directory.GetCurrentDirectory();

        // Recursively load all .gitignore files
        var gitignoreFiles = Directory.EnumerateFiles(currentWorkingDir, ".gitignore", SearchOption.AllDirectories);
        foreach (var gitignoreFile in gitignoreFiles)
        {
            var patterns = File.ReadAllLines(gitignoreFile)
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                .ToList();

            gitignorePatterns.AddRange(patterns);
        }

        // TODO: load git ignore from embedded resource

        return gitignorePatterns.Distinct().ToList();
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
