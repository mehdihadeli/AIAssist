using System.Reflection;

namespace AIAssist.Contracts;

public interface IFileService
{
    /// <summary>
    /// Renders a template file by replacing placeholders with specified replacement values.
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="fullResourceName"></param>
    /// <param name="replacements">An optional object containing key-value pairs for placeholders in the template.</param>
    /// <returns>The rendered template content with placeholders replaced by the specified values, or the original template content if no replacements are provided.</returns>
    string RenderEmbeddedResource(Assembly assembly, string fullResourceName, object? replacements = null);

    bool IsPathIgnored(string path);
}
