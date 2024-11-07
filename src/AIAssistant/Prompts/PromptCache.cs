using AIAssistant.Contracts;
using AIAssistant.Models;
using Clients.Models;
using Humanizer;

namespace AIAssistant.Prompts;

public class PromptCache : IPromptCache
{
    private readonly IList<PromptInformation> _promptInformation = new List<PromptInformation>();

    public void AddPrompt(string embeddedResourceName, CommandType commandType, CodeDiffType? diffType)
    {
        if (_promptInformation.Any(x => x.CommandType == commandType && x.DiffType == diffType))
        {
            throw new Exception(
                $"There is a template for `{
                    commandType.Humanize()
                }` command-type and {
                    diffType?.Humanize()
                } diff-type in the template list storage."
            );
        }
        _promptInformation.Add(new PromptInformation(embeddedResourceName, commandType, diffType));
    }

    public string GetPrompt(CommandType commandType, CodeDiffType? diffType, object? parameters)
    {
        var prompt = _promptInformation.SingleOrDefault(x => x.CommandType == commandType && x.DiffType == diffType);

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
