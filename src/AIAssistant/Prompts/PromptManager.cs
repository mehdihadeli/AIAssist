using BuildingBlocks.Utils;

namespace AIAssistant.Prompts;

public static class PromptManager
{
    public static string RenderPromptTemplate(string promptTemplateName, object? replacements)
    {
        string processedTemplate = FilesUtilities.RenderTemplate(
            PromptConstants.PromptsTemplates,
            $"{promptTemplateName.ToLowerInvariant()}.template",
            replacements
        );

        return processedTemplate;
    }
}
