using System.Text.RegularExpressions;

namespace Clients.Prompts;

public static class PromptManager
{
    public static string RenderPromptTemplate(string promptTemplateName, object? replacements)
    {
        string templateFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            PromptConstants.PromptsTemplates,
            $"{promptTemplateName.ToLowerInvariant()}.template"
        );

        string templateContent = File.ReadAllText(templateFilePath);

        if (replacements == null)
            return templateContent;

        string processedTemplate = ReplacePlaceholders(templateContent, replacements);

        return processedTemplate;
    }

    private static string ReplacePlaceholders(string template, object replacements)
    {
        // Use reflection to get properties of the anonymous type
        var replacementProperties = replacements.GetType().GetProperties();

        return Regex.Replace(
            template,
            @"{{\s*(\w+)\s*}}",
            match =>
            {
                var key = match.Groups[1].Value.ToLowerInvariant();

                // Find the property that matches the key
                foreach (var prop in replacementProperties)
                {
                    if (prop.Name.ToLowerInvariant().Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        return prop.GetValue(replacements)?.ToString() ?? match.Value;
                    }
                }

                // If no matching property is found, return the original placeholder
                return match.Value;
            }
        );
    }
}
