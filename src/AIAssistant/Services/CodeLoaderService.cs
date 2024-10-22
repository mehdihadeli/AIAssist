using BuildingBlocks.Utils;
using TreeSitter.Bindings.CustomTypes.TreeParser;
using TreeSitter.Bindings.Utilities;

namespace AIAssistant.Services;

public class CodeLoaderService
{
    public IReadOnlyList<DefinitionCaptureItem> LoadTreeSitterCodeCaptures(
        string? contextWorkingDir,
        IEnumerable<string>? extraFiles = null
    )
    {
        if (string.IsNullOrEmpty(contextWorkingDir) && extraFiles is null)
            return new List<DefinitionCaptureItem>();

        var applicationCodes = ReadCodeFiles(contextWorkingDir, extraFiles);

        var codeFileMapping = new TreeSitterCodeCaptures();
        var treeSitterCodeCaptures = codeFileMapping.CreateTreeSitterMap(applicationCodes);

        return treeSitterCodeCaptures;
    }

    private IList<CodeFile> ReadCodeFiles(string? contextWorkingDir, IEnumerable<string>? extraFiles)
    {
        List<string> allFiles = new List<string>();

        if (!string.IsNullOrEmpty(contextWorkingDir))
        {
            allFiles.AddRange(Directory.GetFiles(contextWorkingDir, "*", SearchOption.AllDirectories));
        }

        if (extraFiles is not null)
        {
            allFiles.AddRange(extraFiles);
        }

        var applicationCodes = new List<CodeFile>();

        foreach (var file in allFiles)
        {
            if (FilesUtilities.IsIgnored(file))
                continue;
            var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), file);

            var fileContent = File.ReadAllText(file);

            applicationCodes.Add(new CodeFile(fileContent, relativePath));
        }

        return applicationCodes;
    }
}
