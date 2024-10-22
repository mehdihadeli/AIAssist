using System.Globalization;
using System.Reflection;
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

    /// <summary>
    /// Renders a template file by replacing placeholders with specified replacement values.
    /// </summary>
    /// <param name="templateFolder">The folder containing the template file.</param>
    /// <param name="templateName">The name of the template file to be rendered.</param>
    /// <param name="replacements">An optional object containing key-value pairs for placeholders in the template.</param>
    /// <returns>The rendered template content with placeholders replaced by the specified values, or the original template content if no replacements are provided.</returns>
    public static string RenderTemplate(string templateFolder, string templateName, object? replacements = null)
    {
        string templateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, templateFolder, templateName);

        string templateContent = File.ReadAllText(templateFilePath);

        if (replacements == null)
            return templateContent;

        string processedTemplate = ReplacePlaceholders(templateContent, replacements);

        return processedTemplate;
    }

    /// <summary>
    /// Renders an embedded template resource by replacing placeholders with specified replacement values.
    /// </summary>
    /// <param name="assembly">The assembly that contains the embedded template resource.</param>
    /// <param name="fullResourceName">The full name of the embedded resource, including the namespace and any folders, following the pattern:
    /// <c>[DefaultNamespace].[Folder].[Subfolder].[FileName]</c>.</param>
    /// <param name="replacements">An optional object containing key-value pairs for placeholders in the template.</param>
    /// <returns>The rendered template content with placeholders replaced by the specified values, or the original template content if no replacements are provided.</returns>
    public static string RenderEmbeddedTemplate(Assembly assembly, string fullResourceName, object? replacements = null)
    {
        string templateContent = ReadEmbeddedResource(assembly, fullResourceName);

        if (replacements == null)
            return templateContent;

        string processedTemplate = ReplacePlaceholders(templateContent, replacements);

        return processedTemplate;
    }

    /// <summary>
    /// Determines whether the specified file path should be ignored based on predefined ignore patterns.
    /// </summary>
    /// <param name="path">The file path to check against the ignore patterns.</param>
    /// <returns>
    /// <c>true</c> if the file path matches any of the ignore patterns; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// The method compares the given file path against a list of predefined ignore patterns.
    /// If the file path matches any of these patterns, it will be considered ignored.
    /// </remarks>
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

    public static bool IsDirectory(string path)
    {
        return Directory.Exists(path);
    }

    public static bool IsFile(string path)
    {
        return File.Exists(path);
    }

    /// <summary>
    /// Reads the content of an embedded resource from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly that contains the embedded resource.</param>
    /// <param name="fullResourceName">The full name of the resource, including the namespace and any folders, following the pattern:
    /// <c>[RootNamespace].[Folder].[Subfolder].[FileName]</c>.</param>
    /// <returns>A string containing the content of the embedded resource, or null if the resource is not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="assembly"/> or <paramref name="fullResourceName"/> is null.</exception>
    /// <exception cref="IOException">Thrown if there is an error reading the resource stream.</exception>
    public static string ReadEmbeddedResource(Assembly assembly, string fullResourceName)
    {
        using Stream? stream = assembly.GetManifestResourceStream(fullResourceName);

        if (stream == null)
        {
            throw new FileNotFoundException($"Resource {fullResourceName} not found.");
        }

        using StreamReader reader = new StreamReader(stream);

        var content = reader.ReadToEnd();

        return content;
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

        //load git ignore from embedded resource
        var embeddedResourceGitIgnorePatterns = ReadEmbeddedResourceGitIgnore();
        gitignorePatterns.AddRange(embeddedResourceGitIgnorePatterns);

        // Recursively load all .gitignore files from working directory and all of its levels
        var gitignoreFiles = Directory.EnumerateFiles(currentWorkingDir, ".gitignore", SearchOption.AllDirectories);
        foreach (var gitignoreFile in gitignoreFiles)
        {
            var patterns = File.ReadAllLines(gitignoreFile)
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                .ToList();

            gitignorePatterns.AddRange(patterns);
        }

        return gitignorePatterns.Distinct().ToList();
    }

    private static IList<string> ReadEmbeddedResourceGitIgnore()
    {
        var gitIgnore = ".gitignore";
        var rootNamespace = nameof(BuildingBlocks);

        var fullResourceName = $"{rootNamespace}.{gitIgnore}";

        var assembly = Assembly.GetExecutingAssembly();

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
