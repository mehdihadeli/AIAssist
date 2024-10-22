using System.Reflection;
using BuildingBlocks.Utils;

namespace AIAssistant.Prompts;

public static class PromptManager
{
    /// <summary>
    /// Renders a prompt template by replacing placeholders with specified replacement values.
    /// </summary>
    /// <param name="promptTemplateName">The name of the prompt template to be rendered, without the file extension.</param>
    /// <param name="replacements">An optional object containing key-value pairs for placeholders in the template.</param>
    /// <returns>The rendered prompt template content with placeholders replaced by the specified values, or the original template content if no replacements are provided.</returns>
    public static string RenderPromptTemplate(string promptTemplateName, object? replacements)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var templateName = $"{promptTemplateName.ToLowerInvariant()}.template";
        var fullResourceName = $"{nameof(AIAssistant)}.{PromptConstants.PromptsTemplates}.{templateName}";

        // Render the embedded template
        string processedTemplate = FilesUtilities.RenderEmbeddedTemplate(assembly, fullResourceName, replacements);

        return processedTemplate;
    }
}
