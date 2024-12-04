using AIAssist.Models;

namespace AIAssist.Contracts;

public interface IContextService
{
    Context GetContext();
    IList<FileItemContext> GetAllFiles();
    IList<FileItemContext> GetFiles(IList<string>? filesRelativePath);
    void ValidateLoadedFilesLimit();

    /// <summary>
    /// AddOrUpdate folders, sub-folders, files in root level with summary mode.
    /// </summary>
    /// <param name="rootContextFolder"></param>
    void AddContextFolder(string rootContextFolder);

    /// <summary>
    /// AddOrUpdate folders with full files in root level with definition mode.
    /// </summary>
    /// <param name="rootFoldersRelativePath"></param>
    void AddOrUpdateFolder(IList<string>? rootFoldersRelativePath);
    void RemoveFolder(IList<string>? rootFoldersRelativePath);

    /// <summary>
    /// AddOrUpdate files with full definition in all levels
    /// </summary>
    /// <param name="filesRelativePath"></param>
    void AddOrUpdateFiles(IList<string>? filesRelativePath);
    void RemoveFiles(IList<string>? filesRelativePath);
    void AddOrUpdateUrls(IList<string>? urls);
}
