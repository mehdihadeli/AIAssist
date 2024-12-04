using AIAssist.Contracts;
using AIAssist.Models.Options;
using BuildingBlocks.Utils;
using Microsoft.Extensions.Options;
using TreeSitter.Bindings.Contracts;
using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssist.Services;

public class CodeFilesTreeGeneratorService(
    ITreeSitterCodeCaptureService treeSitterCodeCaptureService,
    ITreeStructureGeneratorService treeStructureGeneratorService,
    IOptions<AppOptions> appOptions
) : ICodeFileTreeGeneratorService
{
    private readonly AppOptions _appOptions = appOptions.Value;
    private readonly IDictionary<string, CodeFileMap> _codeFilesMap = new Dictionary<string, CodeFileMap>();

    public IList<CodeFileMap> GetCodeFilesMap(IList<string> files)
    {
        if (!files.Any())
            return new List<CodeFileMap>();

        var result = _codeFilesMap
            .Where(codeFileMap => files.Any(file => file.NormalizePath() == codeFileMap.Key.NormalizePath()))
            .Select(x => x.Value)
            .ToList();

        return result;
    }

    public CodeFileMap? GetCodeFileMap(string file)
    {
        return GetCodeFilesMap([file]).FirstOrDefault();
    }

    public IList<CodeFileMap> AddContextCodeFilesMap(IList<string> files)
    {
        var codeFiles = ReadCodeFiles(files);

        return AddOrUpdateCodeFilesMap(codeFiles, true).ToList();
    }

    public IList<CodeFileMap> AddOrUpdateCodeFilesMap(IList<string> files)
    {
        var codeFiles = ReadCodeFiles(files);

        return AddOrUpdateCodeFilesMap(codeFiles, false).ToList();
    }

    public CodeFileMap? AddOrUpdateCodeFileMap(string file)
    {
        return AddOrUpdateCodeFilesMap([file]).FirstOrDefault();
    }

    private IEnumerable<CodeFileMap> AddOrUpdateCodeFilesMap(IList<CodeFile> codeFiles, bool useShortSummary = false)
    {
        foreach (var codeFile in codeFiles)
        {
            yield return AddOrUpdateCodeFileMap(codeFile, useShortSummary);
        }
    }

    // Generates and adds CodeFileMap to cache with expiration based on AppOptions
    private CodeFileMap AddOrUpdateCodeFileMap(CodeFile codeFile, bool useShortSummary)
    {
        var definitions = treeSitterCodeCaptureService.CreateTreeSitterMap(new List<CodeFile> { codeFile });

        var fileCapturesGroup = definitions
            .Where(def => def.RelativePath.NormalizePath() == codeFile.RelativePath.NormalizePath())
            .ToList();

        if (fileCapturesGroup.Count == 0)
            throw new KeyNotFoundException("Definition not found for specified path");

        var originalCode = treeStructureGeneratorService.GenerateOriginalCodeTree(
            GetOriginalCode(fileCapturesGroup),
            codeFile.RelativePath.NormalizePath()
        );

        var codeFileMap = new CodeFileMap
        {
            RelativePath = codeFile.RelativePath.NormalizePath(),
            Path = codeFile.Path,
            OriginalCode = GetOriginalCode(fileCapturesGroup),
            TreeSitterFullCode = treeStructureGeneratorService.GenerateTreeSitter(fileCapturesGroup, true),
            TreeOriginalCode = originalCode,
            TreeSitterSummarizeCode = useShortSummary
                ? treeStructureGeneratorService.GenerateTreeSitter(fileCapturesGroup, false)
                : originalCode,
        };

        if (_codeFilesMap.Any(x => x.Key.NormalizePath() == codeFile.RelativePath.NormalizePath()))
        {
            _codeFilesMap.Remove(codeFile.RelativePath.NormalizePath());
            _codeFilesMap.Add(codeFileMap.RelativePath.NormalizePath(), codeFileMap);
        }
        else
        {
            _codeFilesMap.Add(codeFileMap.RelativePath.NormalizePath(), codeFileMap);
        }

        return codeFileMap;
    }

    private IList<CodeFile> ReadCodeFiles(IList<string>? files)
    {
        ArgumentException.ThrowIfNullOrEmpty(_appOptions.ContextWorkingDirectory);

        if (files is null)
            return new List<CodeFile>();

        var applicationCodes = new List<CodeFile>();

        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(_appOptions.ContextWorkingDirectory, file);
            var fileContent = File.ReadAllText(file);

            applicationCodes.Add(new CodeFile(fileContent, relativePath.NormalizePath(), file));
        }

        return applicationCodes;
    }

    private static string GetOriginalCode(List<DefinitionCapture> definitionItems)
    {
        return definitionItems.First().OriginalCode;
    }
}
