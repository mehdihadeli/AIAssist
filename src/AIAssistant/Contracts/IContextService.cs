using AIAssistant.Models;

namespace AIAssistant.Contracts;

public interface IContextService
{
    Context GetContext();
    IList<FileItemContext> GetAllFiles();
    IList<FileItemContext> GetFiles(IList<string>? filesRelativePath);
    void AddContextFolder(string contextFolder);
    void AddOrUpdateFolder(IList<string>? foldersRelativePath);
    void AddOrUpdateFiles(IList<string>? filesRelativePath);
    void AddOrUpdateUrls(IList<string>? urls);
}
