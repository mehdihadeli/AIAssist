using System.Text;
using AIAssistant.Models;

namespace AIAssistant.Prompts;

public static class SharedPrompts
{
    public const string CodeBlockFormatIsNotCorrect = "Your `code block format` is  incorrect!";

    public static string FilesAddedToChat(IEnumerable<string> fullFileContents)
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

    public static string AskMoreContextPrompt()
    {
        var renderBlock = PromptManager.RenderPromptTemplate(AIAssistantConstants.Prompts.AskMoreContext, null);

        return renderBlock;
    }

    public static string AddCodeBlock(string treeSitterCode)
    {
        var renderBlock = PromptManager.RenderPromptTemplate(
            AIAssistantConstants.Prompts.CodeBlockTemplate,
            new { treeSitterCode = treeSitterCode }
        );

        return renderBlock;
    }

    public static string AddEmbeddingInputString(string treeSitterCode)
    {
        return PromptManager.RenderPromptTemplate(
            AIAssistantConstants.Prompts.CodeEmbeddingTemplate,
            new { treeSitterCode = treeSitterCode }
        );
    }

    public static string CreateLLMContext(IEnumerable<CodeEmbedding> relevantCode)
    {
        return string.Join(
            Environment.NewLine,
            relevantCode.Select(rc =>
                PromptManager.RenderPromptTemplate(
                    AIAssistantConstants.Prompts.CodeBlockTemplate,
                    new { treeSitterCode = rc.TreeOriginalCode }
                )
            )
        );
    }

    public static string CreateLLMContext(IEnumerable<CodeSummary> codeFileSummaries)
    {
        return string.Join(
            Environment.NewLine,
            codeFileSummaries.Select(codeFileSummary =>
                PromptManager.RenderPromptTemplate(
                    AIAssistantConstants.Prompts.CodeBlockTemplate,
                    new
                    {
                        treeSitterCode = codeFileSummary.UseFullCodeFile
                            ? codeFileSummary.TreeOriginalCode
                            : codeFileSummary.TreeSitterSummarizeCode,
                    }
                )
            )
        );
    }
}
