using AIAssistant.Contracts;
using AIAssistant.Models.Options;
using BuildingBlocks.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TreeSitter.Bindings.Contracts;
using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssistant.Services;

public class CodeFilesTreeGeneratorService(
    ITreeSitterCodeCaptureService treeSitterCodeCaptureService,
    ITreeStructureGeneratorService treeStructureGeneratorService,
    IMemoryCache memoryCache,
    IOptions<AppOptions> appOptions
) : ICodeFileTreeGeneratorService
{
    private readonly AppOptions _appOptions = appOptions.Value;

    public IEnumerable<CodeFileMap> GetOrAddCodeTreeMapFromFiles(string contextWorkingDir, IList<string>? files = null)
    {
        if (string.IsNullOrEmpty(contextWorkingDir) && files is null)
            return new List<CodeFileMap>();

        var codeFiles = ReadCodeFiles(_appOptions.AutoContextEnabled, contextWorkingDir, files);
        var codeFileMaps = new List<CodeFileMap>();

        foreach (var codeFile in codeFiles)
        {
            if (memoryCache.TryGetValue(codeFile.RelativePath, out CodeFileMap? cachedCodeFileMap))
            {
                if (cachedCodeFileMap is not null)
                {
                    codeFileMaps.Add(cachedCodeFileMap);
                }
            }
            else
            {
                var updatedCodeFileMap = AddOrUpdateCodeFilesTreeMap(codeFile);
                codeFileMaps.Add(updatedCodeFileMap);
            }
        }

        return codeFileMaps;
    }

    public IEnumerable<CodeFileMap> AddOrUpdateCodeTreeMapFromFiles(IList<string>? files)
    {
        if (files is null)
            return new List<CodeFileMap>();

        var codeFiles = ReadCodeFiles(false, _appOptions.ContextWorkingDirectory, files);

        return AddOrUpdateCodeFilesTreeMap(codeFiles);
    }

    private IEnumerable<CodeFileMap> AddOrUpdateCodeFilesTreeMap(IList<CodeFile> codeFiles)
    {
        foreach (var codeFile in codeFiles)
        {
            yield return AddOrUpdateCodeFilesTreeMap(codeFile);
        }
    }

    // Generates and adds CodeFileMap to cache with expiration based on AppOptions
    private CodeFileMap AddOrUpdateCodeFilesTreeMap(CodeFile codeFile)
    {
        var definitions = treeSitterCodeCaptureService.CreateTreeSitterMap(new List<CodeFile> { codeFile });

        var fileCapturesGroup = definitions.Where(def => def.RelativePath == codeFile.RelativePath).ToList();

        if (fileCapturesGroup.Count == 0)
            throw new KeyNotFoundException("Definition not found for specified path");

        var codeFileMap = new CodeFileMap
        {
            RelativePath = codeFile.RelativePath,
            OriginalCode = GetOriginalCode(fileCapturesGroup),
            TreeSitterFullCode = treeStructureGeneratorService.GenerateTreeSitter(fileCapturesGroup, true),
            TreeOriginalCode = treeStructureGeneratorService.GenerateOriginalCodeTree(
                GetOriginalCode(fileCapturesGroup),
                codeFile.RelativePath
            ),
            TreeSitterSummarizeCode = treeStructureGeneratorService.GenerateTreeSitter(fileCapturesGroup, false),
            ReferencedCodesMap = GenerateRelatedCodeFilesMap(fileCapturesGroup),
        };

        // Set the expiration based on CacheExpirationInMinutes from AppOptions
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_appOptions.CacheExpirationInMinutes),
        };

        // update value for key if exist otherwise it add value for this key
        memoryCache.Set(codeFileMap.RelativePath, codeFileMap, cacheOptions);

        return codeFileMap;
    }

    private static IEnumerable<ReferencedCodeMap> GenerateRelatedCodeFilesMap(
        List<DefinitionCaptureItem> definitionItems
    )
    {
        // Collect related references for each DefinitionCaptureItem
        return definitionItems
            .SelectMany(item =>
                item.DefinitionCaptureReferences.Select(reference => new ReferencedCodeMap
                {
                    RelativePath = reference.RelativePath,
                    ReferencedValue = reference.ReferencedValue,
                    ReferencedUsage = reference.ReferencedUsage,
                })
            )
            .Distinct()
            .ToList();
    }

    private IList<CodeFile> ReadCodeFiles(bool autoContextEnabled, string contextWorkingDir, IList<string>? extraFiles)
    {
        List<string> allFiles = new List<string>();

        if (string.IsNullOrEmpty(contextWorkingDir))
        {
            contextWorkingDir = _appOptions.ContextWorkingDirectory;
        }

        if (autoContextEnabled)
        {
            allFiles.AddRange(Directory.GetFiles(contextWorkingDir, "*", SearchOption.AllDirectories));
        }

        if (extraFiles is not null && extraFiles.Any())
        {
            allFiles.AddRange(extraFiles);
        }

        var applicationCodes = new List<CodeFile>();

        foreach (var file in allFiles)
        {
            if (FilesUtilities.IsIgnored(file))
                continue;

            var relativePath = Path.GetRelativePath(contextWorkingDir, file);

            var fileContent = File.ReadAllText(file);

            applicationCodes.Add(new CodeFile(fileContent, relativePath));
        }

        return applicationCodes;
    }

    private static string GetOriginalCode(List<DefinitionCaptureItem> definitionItems)
    {
        return definitionItems.First().OriginalCode;
    }
}
