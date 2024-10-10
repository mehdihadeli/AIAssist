using AIAssistant.Models;
using BuildingBlocks.Utils;

namespace AIAssistant.Services;

public class CodeLoaderService
{
    public IEnumerable<ApplicationCode> LoadApplicationCodes(string contextWorkingDir)
    {
        if (string.IsNullOrEmpty(contextWorkingDir))
            return new List<ApplicationCode>();

        var files = Directory.GetFiles(contextWorkingDir, "*", SearchOption.AllDirectories);
        var applicationCodes = new List<ApplicationCode>();

        foreach (var file in files)
        {
            if (FilesUtilities.IsIgnored(file))
                continue;
            var relativePath = Path.GetRelativePath(contextWorkingDir, file);

            var fileContent = File.ReadAllText(file);

            applicationCodes.Add(CreateApplicationCode(fileContent, relativePath));
        }

        return applicationCodes;
    }

    private static ApplicationCode CreateApplicationCode(string code, string relativeFilePath)
    {
        var applicationCode = new ApplicationCode(code, relativeFilePath);

        return applicationCode;
    }
}
