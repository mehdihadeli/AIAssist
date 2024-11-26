using AIAssist.Models;
using Clients.Models;

namespace AIAssist.Contracts;

public interface IPromptManager
{
    /// <summary>
    /// Renders a prompt template by replacing placeholders with specified replacement values.
    /// </summary>
    /// <param name="promptTemplateName">The name of the prompt template to be rendered, without the file extension.</param>
    /// <param name="replacements">An optional object containing key-value pairs for placeholders in the template.</param>
    /// <returns>The rendered prompt template content with placeholders replaced by the specified values, or the original template content if no replacements are provided.</returns>
    string RenderPromptTemplate(string promptTemplateName, object? replacements);
    void AddPrompt(string embeddedResourceName, CommandType commandType, CodeDiffType? diffType);
    string GetPrompt(CommandType commandType, CodeDiffType? diffType, object? parameters);
    string AddCodeBlock(string treeSitterCode);
    string GetEmbeddingInputString(string treeSitterCode);
    string CreateLLMContext(IEnumerable<string> codeBlocks);
    string FilesAddedToChat(IEnumerable<string> fullFileContents);
    string? GetSystemPrompt(IList<string>? codes, CodeAssistType codeAssistType, CodeDiffType diffType);
}
