using AIAssistant.Models;

namespace AIAssistant.Contracts;

public interface IPromptStorage
{
    void AddPrompt(string embeddedResourceName, CommandType commandType, DiffType? diffType);
    string GetPrompt(CommandType commandType, DiffType? diffType, object? parameters);
}
