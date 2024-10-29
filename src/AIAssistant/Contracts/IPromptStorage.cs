using AIAssistant.Models;

namespace AIAssistant.Contracts;

public interface IPromptStorage
{
    void AddPrompt(string embeddedResourceName, CommandType commandType, CodeDiffType? diffType);
    string GetPrompt(CommandType commandType, CodeDiffType? diffType, object? parameters);
}
