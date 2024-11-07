using AIAssistant.Models;
using Clients.Models;

namespace AIAssistant.Contracts;

public interface IPromptCache
{
    void AddPrompt(string embeddedResourceName, CommandType commandType, CodeDiffType? diffType);
    string GetPrompt(CommandType commandType, CodeDiffType? diffType, object? parameters);
}
