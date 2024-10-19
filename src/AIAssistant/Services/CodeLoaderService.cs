using BuildingBlocks.Utils;
using TreeSitter.Bindings.CustomTypes.TreeParser;
using TreeSitter.Bindings.Utilities;

namespace AIAssistant.Services;

public class CodeLoaderService
{
    public IEnumerable<CodeFile> LoadApplicationCodes(string contextWorkingDir)
    {
        if (string.IsNullOrEmpty(contextWorkingDir))
            return new List<CodeFile>();

        var files = Directory.GetFiles(contextWorkingDir, "*", SearchOption.AllDirectories);
        var applicationCodes = new List<CodeFile>();

        foreach (var file in files)
        {
            if (FilesUtilities.IsIgnored(file))
                continue;
            var relativePath = Path.GetRelativePath(contextWorkingDir, file);

            var fileContent = File.ReadAllText(file);

            applicationCodes.Add(new CodeFile(fileContent, relativePath));
        }

        var codeFileMapping = new CodeFileMappings();
        var mappedCodes = codeFileMapping.CreateTreeSitterMap(applicationCodes);

        return applicationCodes;
    }
}
