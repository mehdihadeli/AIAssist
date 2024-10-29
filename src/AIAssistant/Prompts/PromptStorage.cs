using AIAssistant.Contracts;
using AIAssistant.Models;
using Humanizer;

namespace AIAssistant.Prompts;

public class PromptStorage : IPromptStorage
{
    private readonly IList<PromptInformation> _promptInformations = new List<PromptInformation>();

    public void AddPrompt(string embeddedResourceName, CommandType commandType, CodeDiffType? diffType)
    {
        if (_promptInformations.Any(x => x.CommandType == commandType && x.DiffType == diffType))
        {
            throw new Exception(
                $"There is a template for `{
                    commandType.Humanize()
                }` command-type and {
                    diffType?.Humanize()
                } diff-type in the template list storage."
            );
        }
        _promptInformations.Add(new PromptInformation(embeddedResourceName, commandType, diffType));
    }

    public string GetPrompt(CommandType commandType, CodeDiffType? diffType, object? parameters)
    {
        var prompt = _promptInformations.SingleOrDefault(x => x.CommandType == commandType && x.DiffType == diffType);

        if (prompt is null)
            throw new Exception(
                $"prompt not found for {
                    commandType.Humanize()
                } command-type and {
                    diffType?.Humanize()
                } diff-type."
            );

        var promptTemplate = PromptManager.RenderPromptTemplate(prompt.EmbeddedResourceName, parameters);

        return promptTemplate;
    }
}

public record PromptInformation(string EmbeddedResourceName, CommandType CommandType, CodeDiffType? DiffType);
