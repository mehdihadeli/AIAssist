using AIAssistant.Contracts;
using AIAssistant.Models;
using AIAssistant.Models.Options;
using BuildingBlocks.Utils;
using Microsoft.Extensions.Options;

namespace AIAssistant.Services;

public class ContextService(
    IOptions<AppOptions> appOptions,
    IFileService fileService,
    ICodeFileTreeGeneratorService codeFileTreeGeneratorService
) : IContextService
{
    private readonly AppOptions _appOptions = appOptions.Value;
    private readonly Context _currentContext = new();

    public void AddContextFolder(string contextFolder)
    {
        var contextWorkingDir = _appOptions.ContextWorkingDirectory;
        ArgumentException.ThrowIfNullOrEmpty(contextWorkingDir);

        var foldersItemContext = InitFoldersItemContext([contextFolder], contextFolder, true);
        foreach (var folderItemContext in foldersItemContext)
        {
            _currentContext.ContextItems.Add(folderItemContext);
        }

        ValidateLoadedFilesLimit();
    }

    public void AddOrUpdateFolder(IList<string>? foldersRelativePath)
    {
        if (foldersRelativePath is null || foldersRelativePath.Count == 0)
            return;

        var contextWorkingDir = _appOptions.ContextWorkingDirectory;
        ArgumentException.ThrowIfNullOrEmpty(contextWorkingDir);

        var folders = foldersRelativePath
            .Select(folder => Path.Combine(contextWorkingDir, folder.NormalizePath()))
            .ToList();

        var foldersItemsContext = InitFoldersItemContext(folders, contextWorkingDir, false);

        foreach (var folderItemsContext in foldersItemsContext)
        {
            var existingItem = _currentContext
                .ContextItems.OfType<FolderItemContext>()
                .FirstOrDefault(x => x.RelativePath != folderItemsContext.RelativePath.NormalizePath());

            if (existingItem is null)
            {
                _currentContext.ContextItems.Add(folderItemsContext);
            }
            else
            {
                _currentContext.ContextItems.Remove(existingItem);
                _currentContext.ContextItems.Add(folderItemsContext);
            }
        }

        ValidateLoadedFilesLimit();
    }

    public void AddOrUpdateFiles(IList<string>? filesRelativePath)
    {
        if (filesRelativePath is null || filesRelativePath.Count == 0)
            return;

        var contextWorkingDir = _appOptions.ContextWorkingDirectory;
        ArgumentException.ThrowIfNullOrEmpty(contextWorkingDir);

        var files = filesRelativePath.Select(file => Path.Combine(contextWorkingDir, file.NormalizePath())).ToList();
        var filesItemsContext = InitFilesItemContext(files, contextWorkingDir, false);

        foreach (var fileItemContext in filesItemsContext)
        {
            var existingItem = _currentContext
                .ContextItems.OfType<FileItemContext>()
                .FirstOrDefault(x => x.RelativePath != fileItemContext.RelativePath.NormalizePath());

            if (existingItem is null)
            {
                _currentContext.ContextItems.Add(fileItemContext);
            }
            else
            {
                _currentContext.ContextItems.Remove(existingItem);
                _currentContext.ContextItems.Add(fileItemContext);
            }
        }

        ValidateLoadedFilesLimit();
    }

    public void AddOrUpdateUrls(IList<string>? urls)
    {
        if (urls is null || urls.Count == 0)
            return;
    }

    public Context GetContext()
    {
        return _currentContext;
    }

    public IList<FileItemContext> GetAllFiles()
    {
        var allFiles = new List<FileItemContext>();

        // Add top-level FileItemContext objects
        allFiles.AddRange(_currentContext.ContextItems.OfType<FileItemContext>());

        // Add files from all FolderItemContext objects recursively
        foreach (var folder in _currentContext.ContextItems.OfType<FolderItemContext>())
        {
            CollectFilesFromFolder(folder, allFiles);
        }

        return allFiles;
    }

    public IList<FileItemContext> GetFiles(IList<string>? filesRelativePath)
    {
        var matchingFiles = new List<FileItemContext>();

        if (filesRelativePath == null || filesRelativePath.Count == 0)
        {
            matchingFiles.AddRange(_currentContext.ContextItems.OfType<FileItemContext>());

            foreach (var folder in _currentContext.ContextItems.OfType<FolderItemContext>())
            {
                CollectFilesFromFolder(folder, matchingFiles);
            }
        }
        else
        {
            // Match specific file paths
            var normalizedPaths = filesRelativePath
                .Select(path => path.NormalizePath())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Check files in top-level context
            matchingFiles.AddRange(
                _currentContext
                    .ContextItems.OfType<FileItemContext>()
                    .Where(file => normalizedPaths.Contains(file.RelativePath.NormalizePath()))
            );

            // Check files in folders
            foreach (var folder in _currentContext.ContextItems.OfType<FolderItemContext>())
            {
                CollectMatchingFiles(folder, normalizedPaths, matchingFiles);
            }
        }

        return matchingFiles;
    }

    public void ValidateLoadedFilesLimit()
    {
        if (GetAllFiles().Count == _appOptions.NumberOfFilesLimit)
        {
            throw new Exception(
                $"File limit count {appOptions.Value.NumberOfFilesLimit} exceeded. You can ignore files and folders that are not necessary with adding them to '.aiassistignore' file or change the level of loading folders by setting 'AppOption.TreeLevel'"
            );
        }
    }

    private void CollectFilesFromFolder(FolderItemContext folder, List<FileItemContext> fileList)
    {
        // Add all files from the folder
        fileList.AddRange(folder.Files);

        // Recursively collect files from subfolders
        foreach (var subFolder in folder.SubFoldersItemContext)
        {
            CollectFilesFromFolder(subFolder, fileList);
        }
    }

    private void CollectMatchingFiles(
        FolderItemContext folder,
        HashSet<string> normalizedPaths,
        List<FileItemContext> matchingFiles
    )
    {
        // Add files that match the paths
        matchingFiles.AddRange(folder.Files.Where(file => normalizedPaths.Contains(file.RelativePath.NormalizePath())));

        // Recursively check subfolders
        foreach (var subFolder in folder.SubFoldersItemContext)
        {
            CollectMatchingFiles(subFolder, normalizedPaths, matchingFiles);
        }
    }

    private IList<FolderItemContext> InitFoldersItemContext(
        IList<string> folders,
        string contextWorkingDir,
        bool useShortSummary
    )
    {
        List<FolderItemContext> foldersItemContext = new List<FolderItemContext>();
        string currentFolder = string.Empty;

        try
        {
            var validFolders = folders.Where(folder => !fileService.IsPathIgnored(folder)).ToList();
            int treeLevel = _appOptions.TreeLevel;

            foreach (var folderPath in validFolders)
            {
                currentFolder = folderPath;

                // Start recursion with current depth set to 1
                var subFoldersItemContext = InitSubFoldersItemContext(
                    folderPath,
                    contextWorkingDir,
                    useShortSummary,
                    1,
                    treeLevel
                );
                var filesItemsContext = InitFilesItemContext(folderPath, contextWorkingDir, useShortSummary);

                var folderRelativePath = Path.GetRelativePath(contextWorkingDir, folderPath).NormalizePath();

                var folderItemContext = new FolderItemContext(
                    folderPath,
                    folderRelativePath,
                    subFoldersItemContext,
                    filesItemsContext
                );

                foldersItemContext.Add(folderItemContext);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied to folder '{currentFolder}': {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"I/O error accessing folder '{currentFolder}': {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading folder contents for '{currentFolder}': {ex.Message}");
        }

        return foldersItemContext;
    }

    private IList<FolderItemContext> InitSubFoldersItemContext(
        string folderPath,
        string contextWorkingDir,
        bool useShortSummary,
        int currentDepth,
        int treeLevel
    )
    {
        IList<FolderItemContext> subFolders = new List<FolderItemContext>();
        string currentSubFolder = string.Empty;

        try
        {
            // Stop recursion if the tree level is exceeded
            if (treeLevel > 0 && currentDepth >= treeLevel)
            {
                // Return empty list as no deeper levels are loaded
                return subFolders;
            }

            foreach (
                var subFolder in Directory
                    .GetDirectories(folderPath)
                    .Where(subFolder => !fileService.IsPathIgnored(subFolder))
            )
            {
                currentSubFolder = subFolder;

                var subFolderFilesItemContext = InitFilesItemContext(subFolder, contextWorkingDir, useShortSummary);
                var subFolderRelativePath = Path.GetRelativePath(contextWorkingDir, subFolder).NormalizePath();

                // Recursive call for each subfolder, incrementing the depth
                var subFolderContext = new FolderItemContext(
                    subFolder,
                    subFolderRelativePath,
                    InitSubFoldersItemContext(
                        subFolder,
                        contextWorkingDir,
                        useShortSummary,
                        currentDepth + 1,
                        treeLevel
                    ),
                    subFolderFilesItemContext
                );

                subFolders.Add(subFolderContext);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied to folder '{currentSubFolder}': {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"I/O error accessing folder '{currentSubFolder}': {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading folder contents for '{currentSubFolder}': {ex.Message}");
        }

        return subFolders;
    }

    private IList<FileItemContext> InitFilesItemContext(
        IList<string> files,
        string contextWorkingDir,
        bool useShortSummary
    )
    {
        string currentFile = string.Empty;
        List<FileItemContext> filesItemContext = new List<FileItemContext>();

        try
        {
            var validFiles = files.Where(file => !fileService.IsPathIgnored(file)).ToList();

            if (useShortSummary)
            {
                codeFileTreeGeneratorService.AddContextCodeFilesMap(validFiles);
            }
            else
            {
                codeFileTreeGeneratorService.AddOrUpdateCodeFilesMap(validFiles);
            }

            foreach (var file in validFiles)
            {
                currentFile = file;
                var fileRelativePath = Path.GetRelativePath(contextWorkingDir, file).NormalizePath();
                var codeFileMap = codeFileTreeGeneratorService.GetCodeFileMap(fileRelativePath);
                if (codeFileMap is null)
                    continue;

                filesItemContext.Add(new FileItemContext(file, fileRelativePath, codeFileMap));
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied to file '{currentFile}': {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"I/O error accessing file '{currentFile}': {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading file contents for '{currentFile}': {ex.Message}");
        }

        return filesItemContext;
    }

    private IList<FileItemContext> InitFilesItemContext(
        string folderPath,
        string contextWorkingDir,
        bool useShortSummary
    )
    {
        List<FileItemContext> filesItemContext = new List<FileItemContext>();
        string currentFile = string.Empty;

        try
        {
            var validFiles = Directory.GetFiles(folderPath).Where(file => !fileService.IsPathIgnored(file)).ToList();

            if (useShortSummary)
            {
                codeFileTreeGeneratorService.AddContextCodeFilesMap(validFiles);
            }
            else
            {
                codeFileTreeGeneratorService.AddOrUpdateCodeFilesMap(validFiles);
            }

            foreach (var file in validFiles)
            {
                currentFile = file;
                var fileRelativePath = Path.GetRelativePath(contextWorkingDir, file).NormalizePath();
                var codeFileMap = codeFileTreeGeneratorService.GetCodeFileMap(fileRelativePath);
                if (codeFileMap is null)
                    continue;

                filesItemContext.Add(new FileItemContext(file, fileRelativePath, codeFileMap));
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied to file '{currentFile}': {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"I/O error accessing file '{currentFile}': {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading file contents for '{currentFile}': {ex.Message}");
        }

        return filesItemContext;
    }
}
