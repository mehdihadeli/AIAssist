using AIAssist.Models;

namespace AIAssist.Contracts;

public interface IContextService
{
    Context GetContext();
    IList<FileItemContext> GetAllFiles();
    IList<FileItemContext> GetFiles(IList<string>? filesRelativePath);
    void ValidateLoadedFilesLimit();
    void AddContextFolder(string contextFolder);
    void AddOrUpdateFolder(IList<string>? foldersRelativePath);
    void AddOrUpdateFiles(IList<string>? filesRelativePath);
    void AddOrUpdateUrls(IList<string>? urls);
}
