using System.Reflection;
using AIAssist.Contracts;
using BuildingBlocks.Utils;

namespace AIAssist.Services;

public class FileService : IFileService
{
    private static readonly List<string> _gitignorePatterns = new();

    public FileService()
    {
        LoadIgnorePatterns();
    }

    public string RenderEmbeddedResource(Assembly assembly, string fullResourceName, object? replacements = null)
    {
        string templateContent = FilesUtilities.ReadEmbeddedResource(assembly, fullResourceName, replacements);

        return templateContent;
    }

    public bool IsPathIgnored(string path)
    {
        if (FilesUtilities.IsDirectory(path))
        {
            // Check if the path doesn't end with a directory separator and add it, to detecting directory by regex matcher
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                // Add the correct separator for the platform
                path += Path.DirectorySeparatorChar;
            }
        }

        var relativeToRootPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), path);

        // Check if the file itself matches any ignore pattern
        if (FilesUtilities.MatchPattern(relativeToRootPath, _gitignorePatterns))
        {
            return true;
        }

        if (FilesUtilities.IsExecutableFile(relativeToRootPath))
        {
            return true;
        }

        return false;
    }

    private void LoadIgnorePatterns()
    {
        var embeddedResourceIgnorePatterns = FilesUtilities.LoadEmbeddedResourceIgnorePatterns(
            $"{nameof(AIAssist)}..aiassistignore",
            Assembly.GetExecutingAssembly()
        );
        var dirIgnorePatterns = FilesUtilities.LoadIgnorePattersCurrentDirectory([".gitignore", ".aiassistignore"]);

        var mergeIgnore = embeddedResourceIgnorePatterns
            .Union(dirIgnorePatterns, StringComparer.Ordinal)
            .Distinct(StringComparer.Ordinal);
        _gitignorePatterns.AddRange(mergeIgnore);
    }
}
