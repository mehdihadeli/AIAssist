using AIAssist.Contracts;
using AIAssist.Models;
using AIAssist.Models.Options;
using BuildingBlocks.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssist.Services;

public class ContextService(
    IOptions<AppOptions> appOptions,
    IFileService fileService,
    ICodeFileTreeGeneratorService codeFileTreeGeneratorService,
    ILogger<ContextService> logger
) : IContextService
{
    private readonly AppOptions _appOptions = appOptions.Value;
    private readonly Context _currentContext = new();

    public void AddContextFolder(string rootContextFolder)
    {
        var contextWorkingDir = _appOptions.ContextWorkingDirectory;
        ArgumentException.ThrowIfNullOrEmpty(contextWorkingDir);

        try
        {
            // traverse and generate folders in the root level based on relative path and summary code
            var foldersItemContext = TraverseAndGenerateFoldersContext([rootContextFolder], rootContextFolder, true);

            foreach (var folderItemContext in foldersItemContext)
            {
                _currentContext.ContextItems.Add(folderItemContext);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogError("Access denied to folder: {Message}", ex.Message);
            throw;
        }
        catch (IOException ex)
        {
            logger.LogError("I/O error while accessing folders: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError("Unexpected error: {Message}", ex.Message);
            throw;
        }

        ValidateLoadedFilesLimit();
    }

    public void AddOrUpdateFolder(IList<string>? rootFoldersRelativePath)
    {
        // check folders in root level
        if (rootFoldersRelativePath is null || rootFoldersRelativePath.Count == 0)
            return;

        try
        {
            var contextWorkingDir = _appOptions.ContextWorkingDirectory;
            ArgumentException.ThrowIfNullOrEmpty(contextWorkingDir);

            var folders = rootFoldersRelativePath
                .Select(folder => Path.Combine(contextWorkingDir, folder.NormalizePath()))
                .ToList();

            // traverse and generate folders in the root level based on relative path
            var traversedFoldersContext = TraverseAndGenerateFoldersContext(folders, contextWorkingDir, false);

            foreach (var traversedFolderContext in traversedFoldersContext)
            {
                // check for existing folders in the context root
                var existingFolderContext = _currentContext
                    .ContextItems.OfType<FolderItemContext>()
                    .FirstOrDefault(x =>
                        x.RelativePath.NormalizePath() == traversedFolderContext.RelativePath.NormalizePath()
                    );

                if (existingFolderContext is null)
                {
                    _currentContext.ContextItems.Add(traversedFolderContext);
                }
                else
                {
                    // Remove the old folder context entirely
                    _currentContext.ContextItems.Remove(existingFolderContext);

                    // Add the new folder context, replacing the old one
                    _currentContext.ContextItems.Add(traversedFolderContext);
                }
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogError("Access denied to folder: {Message}", ex.Message);
            throw;
        }
        catch (IOException ex)
        {
            logger.LogError("I/O error while accessing folders: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError("Unexpected error: {Message}", ex.Message);
            throw;
        }

        ValidateLoadedFilesLimit();
    }

    public void RemoveFolder(IList<string>? rootFoldersRelativePath)
    {
        if (rootFoldersRelativePath is null || rootFoldersRelativePath.Count == 0)
            return;

        var normalizedPaths = rootFoldersRelativePath
            .Select(path => path.NormalizePath())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Remove folders from the root context items
        RemoveFoldersFromContext(_currentContext.ContextItems, normalizedPaths);
    }

    public void AddOrUpdateFiles(IList<string>? filesRelativePath)
    {
        // add or update files in all levels

        if (filesRelativePath is null || filesRelativePath.Count == 0)
            return;

        var contextWorkingDir = _appOptions.ContextWorkingDirectory;
        ArgumentException.ThrowIfNullOrEmpty(contextWorkingDir);

        var files = filesRelativePath.Select(file => Path.Combine(contextWorkingDir, file.NormalizePath())).ToList();

        // traverse and generate files in all levels based on relative path
        var traversedFilesContext = TraverseAndGenerateFilesContext(files, contextWorkingDir, false);

        foreach (var traversedFileContext in traversedFilesContext)
        {
            // check for existing file in the context
            var existingFileContext = GetFiles([traversedFileContext.RelativePath.NormalizePath()]).FirstOrDefault();

            if (existingFileContext is null)
            {
                _currentContext.ContextItems.Add(traversedFileContext);
            }
            else
            {
                // Update the existing file in all levels it appears
                UpdateFileInContext(_currentContext.ContextItems, traversedFileContext);
            }
        }

        ValidateLoadedFilesLimit();
    }

    public void RemoveFiles(IList<string>? filesRelativePath)
    {
        if (filesRelativePath is null || filesRelativePath.Count == 0)
            return;

        var normalizedPaths = filesRelativePath
            .Select(path => path.NormalizePath())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Recursively remove files in the context
        RemoveFilesFromContext(_currentContext.ContextItems, normalizedPaths);
    }

    public void AddOrUpdateUrls(IList<string>? urls)
    {
        throw new NotImplementedException();
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

    private IList<FolderItemContext> TraverseAndGenerateFoldersContext(
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

                // traverse and generate subfolders in this level
                // Start recursion with current depth set to 1
                var subFoldersItemContext = TraverseAndGenerateSubFoldersContext(
                    folderPath,
                    contextWorkingDir,
                    useShortSummary,
                    1,
                    treeLevel
                );

                // traverse and generate files in all levels based on relative path
                var traversedFilesContext = TraverseAndGenerateFilesContext(
                    folderPath,
                    contextWorkingDir,
                    useShortSummary
                );

                var folderRelativePath = Path.GetRelativePath(contextWorkingDir, folderPath).NormalizePath();

                var folderResultContext = new FolderItemContext(
                    folderPath,
                    folderRelativePath,
                    subFoldersItemContext,
                    traversedFilesContext
                );

                foldersItemContext.Add(folderResultContext);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogError("Access denied to folder '{Folder}': {Message}", currentFolder, ex.Message);
            throw;
        }
        catch (IOException ex)
        {
            logger.LogError("I/O error accessing folder '{Folder}': {Message}", currentFolder, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError("Error loading folder contents for '{Folder}': {Message}", currentFolder, ex.Message);
            throw;
        }

        return foldersItemContext;
    }

    private IList<FolderItemContext> TraverseAndGenerateSubFoldersContext(
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
                return subFolders;
            }

            foreach (
                var subFolder in Directory
                    .GetDirectories(folderPath)
                    .Where(subFolder => !fileService.IsPathIgnored(subFolder))
            )
            {
                currentSubFolder = subFolder;

                // traverse and generate files in all levels based on relative path
                var filesItemContext = TraverseAndGenerateFilesContext(subFolder, contextWorkingDir, useShortSummary);

                var subFolderRelativePath = Path.GetRelativePath(contextWorkingDir, subFolder).NormalizePath();

                // Recursive call for each subfolder, incrementing the depth
                var subFolderResultContext = new FolderItemContext(
                    subFolder,
                    subFolderRelativePath,
                    TraverseAndGenerateSubFoldersContext(
                        subFolder,
                        contextWorkingDir,
                        useShortSummary,
                        currentDepth + 1,
                        treeLevel
                    ),
                    filesItemContext
                );

                subFolders.Add(subFolderResultContext);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogError("Access denied to folder '{Folder}': {Message}", currentSubFolder, ex.Message);
            throw;
        }
        catch (IOException ex)
        {
            logger.LogError("I/O error accessing folder '{Folder}': {Message}", currentSubFolder, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError("Error loading folder contents for '{Folder}': {Message}", currentSubFolder, ex.Message);
            throw;
        }

        return subFolders;
    }

    private IList<FileItemContext> TraverseAndGenerateFilesContext(
        IList<string> files,
        string contextWorkingDir,
        bool useShortSummary
    )
    {
        // Generate new updated FileItemContext for file path
        string currentFile = string.Empty;
        List<FileItemContext> filesItemContext = [];

        try
        {
            var validFiles = files.Where(file => !fileService.IsPathIgnored(file)).ToList();

            IList<CodeFileMap> codeFilesMap = useShortSummary
                ? codeFileTreeGeneratorService.AddContextCodeFilesMap(validFiles)
                : codeFileTreeGeneratorService.AddOrUpdateCodeFilesMap(validFiles);

            foreach (var codeFileMap in codeFilesMap)
            {
                var fileRelativePath = Path.GetRelativePath(contextWorkingDir, codeFileMap.RelativePath)
                    .NormalizePath();

                filesItemContext.Add(new FileItemContext(codeFileMap.Path, fileRelativePath, codeFileMap));
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogError("Access denied to file '{Folder}': {Message}", currentFile, ex.Message);
            throw;
        }
        catch (IOException ex)
        {
            logger.LogError("I/O error accessing file '{Folder}': {Message}", currentFile, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError("Error loading file contents for '{Folder}': {Message}", currentFile, ex.Message);
            throw;
        }

        return filesItemContext;
    }

    private IList<FileItemContext> TraverseAndGenerateFilesContext(
        string folderPath,
        string contextWorkingDir,
        bool useShortSummary
    )
    {
        // Generate new updated FileItemContext for file path
        List<FileItemContext> filesItemContext = [];
        string currentFile = string.Empty;

        try
        {
            var validFiles = Directory.GetFiles(folderPath).Where(file => !fileService.IsPathIgnored(file)).ToList();

            // to store and update cache in the tree-sitter map
            IList<CodeFileMap> codeFilesMap = useShortSummary
                ? codeFileTreeGeneratorService.AddContextCodeFilesMap(validFiles)
                : codeFileTreeGeneratorService.AddOrUpdateCodeFilesMap(validFiles);

            foreach (var codeFileMap in codeFilesMap)
            {
                var fileRelativePath = Path.GetRelativePath(contextWorkingDir, codeFileMap.RelativePath)
                    .NormalizePath();

                filesItemContext.Add(new FileItemContext(codeFileMap.Path, fileRelativePath, codeFileMap));
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogError("Access denied to file '{Folder}': {Message}", currentFile, ex.Message);
            throw;
        }
        catch (IOException ex)
        {
            logger.LogError("I/O error accessing file '{Folder}': {Message}", currentFile, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError("Error loading file contents for '{Folder}': {Message}", currentFile, ex.Message);
            throw;
        }

        return filesItemContext;
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

    private void UpdateFileInContext(IList<BaseContextItem> contextItems, FileItemContext newFileContext)
    {
        for (int i = 0; i < contextItems.Count; i++)
        {
            var item = contextItems[i];
            switch (item)
            {
                case FileItemContext fileContext
                    when fileContext.RelativePath.NormalizePath() == newFileContext.RelativePath.NormalizePath():
                    // Replace the file with the updated version
                    contextItems[i] = newFileContext;
                    break;

                case FolderItemContext folderContext:
                    // Check and update files inside this folder
                    UpdateFileInContext(folderContext.Files.Cast<BaseContextItem>().ToList(), newFileContext);

                    // Recursively process subfolders
                    UpdateFileInContext(
                        folderContext.SubFoldersItemContext.Cast<BaseContextItem>().ToList(),
                        newFileContext
                    );
                    break;
            }
        }
    }

    private void RemoveFilesFromContext(IList<BaseContextItem> contextItems, HashSet<string> normalizedPaths)
    {
        for (int i = contextItems.Count - 1; i >= 0; i--)
        {
            if (
                contextItems[i] is FileItemContext fileContext
                && normalizedPaths.Contains(fileContext.RelativePath.NormalizePath())
            )
            {
                // Remove matching file
                contextItems.RemoveAt(i);
            }
            else if (contextItems[i] is FolderItemContext folderContext)
            {
                // Remove files from the folder's files list
                RemoveFilesFromContext(folderContext.Files.Cast<BaseContextItem>().ToList(), normalizedPaths);

                // Recursively remove files from subfolders
                RemoveFilesFromContext(
                    folderContext.SubFoldersItemContext.Cast<BaseContextItem>().ToList(),
                    normalizedPaths
                );
            }
        }
    }

    private void RemoveFoldersFromContext(IList<BaseContextItem> contextItems, HashSet<string> normalizedPaths)
    {
        for (int i = contextItems.Count - 1; i >= 0; i--)
        {
            if (
                contextItems[i] is FolderItemContext currentFolderContext
                && normalizedPaths.Contains(currentFolderContext.RelativePath.NormalizePath())
            )
            {
                // Remove matching folder
                contextItems.RemoveAt(i);
            }
            else if (contextItems[i] is FolderItemContext subFolderContext)
            {
                // Recursively check and remove matching subfolders
                RemoveFoldersFromContext(
                    subFolderContext.SubFoldersItemContext.Cast<BaseContextItem>().ToList(),
                    normalizedPaths
                );
            }
        }
    }
}
