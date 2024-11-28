using System.Reflection;
using System.Text;
using AIAssist.Contracts;
using AIAssist.Models;
using Clients.Models;
using Humanizer;

namespace AIAssist.Prompts;

public class PromptManager(IFileService fileService) : IPromptManager
{
    private readonly IList<PromptInformation> _promptInformation = new List<PromptInformation>();

    /// <summary>
    /// Renders a prompt template by replacing placeholders with specified replacement values.
    /// </summary>
    /// <param name="promptTemplateName">The name of the prompt template to be rendered, without the file extension.</param>
    /// <param name="replacements">An optional object containing key-value pairs for placeholders in the template.</param>
    /// <returns>The rendered prompt template content with placeholders replaced by the specified values, or the original template content if no replacements are provided.</returns>
    public string RenderPromptTemplate(string promptTemplateName, object? replacements)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var templateName = $"{promptTemplateName.ToLowerInvariant()}.template";
        var fullResourceName = $"{nameof(AIAssist)}.{AIAssistConstants.PromptsTemplatesNamespace}.{templateName}";

        // Render the embedded template
        string processedTemplate = fileService.RenderEmbeddedResource(assembly, fullResourceName, replacements);

        return processedTemplate;
    }

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

        var promptTemplate = RenderPromptTemplate(prompt.EmbeddedResourceName, parameters);

        return promptTemplate;
    }

    public string FilesAddedToChat(IEnumerable<string> fullFileContents)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("I added below files content to the context, now can you can use them for your response:");
        sb.Append(Environment.NewLine);

        foreach (var fileContent in fullFileContents)
        {
            sb.AppendLine(fileContent);
            sb.AppendLine(Environment.NewLine);
        }

        return sb.ToString();
    }

    public string? GetSystemPrompt(IList<string>? codes, CodeAssistType codeAssistType, CodeDiffType diffType)
    {
        if (codes is null || codes.Count == 0)
            return null;

        List<string> systemPrompts = new List<string>();

        var systemCodeAssistPrompt = GetPrompt(CommandType.Code, diffType, null);
        if (!string.IsNullOrEmpty(systemCodeAssistPrompt))
            systemPrompts.Add(systemCodeAssistPrompt);

        // Add `ask-more-context-prompt` to the code-assistant prompts when `CodeAssistType` is `Summary`
        var askMoreContextPrompt = codeAssistType == CodeAssistType.Summary ? AskMoreContextPrompt() : string.Empty;
        if (!string.IsNullOrEmpty(askMoreContextPrompt))
            systemPrompts.Add(askMoreContextPrompt);

        var codeContext = CreateLLMContext(codes);
        if (!string.IsNullOrEmpty(codeContext))
            systemPrompts.Add(codeContext);

        var result = string.Join(Environment.NewLine, systemPrompts);

        return result;
    }

    public string AddCodeBlock(string treeSitterCode)
    {
        var renderBlock = RenderPromptTemplate(
            AIAssistConstants.Prompts.CodeBlockTemplate,
            new { treeSitterCode = treeSitterCode }
        );

        return renderBlock;
    }

    public string GetEmbeddingInputString(string treeSitterCode)
    {
        return RenderPromptTemplate(
            AIAssistConstants.Prompts.CodeEmbeddingTemplate,
            new { treeSitterCode = treeSitterCode }
        );
    }

    public string CreateLLMContext(IEnumerable<string> codeBlocks)
    {
        return RenderPromptTemplate(
            AIAssistConstants.Prompts.CodeContextTemplate,
            new { codeContext = string.Join(Environment.NewLine, codeBlocks) }
        );
    }

    private string AskMoreContextPrompt()
    {
        var renderBlock = RenderPromptTemplate(AIAssistConstants.Prompts.AskMoreContext, null);

        return renderBlock;
    }
}
