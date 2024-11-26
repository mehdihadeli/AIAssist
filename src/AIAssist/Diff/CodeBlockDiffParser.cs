using AIAssist.Contracts.Diff;
using AIAssist.Models;

namespace AIAssist.Diff;

public class CodeBlockDiffParser : ICodeDiffParser
{
    public IList<DiffResult> ParseDiffResults(string diffContent, string contextWorkingDirectory)
    {
        var diffResults = new List<DiffResult>();

        while (!string.IsNullOrWhiteSpace(diffContent))
        {
            var (filePath, actionType, remainingDiffContent) = ExtractPathAndAction(diffContent);

            if (filePath == null || actionType == null)
            {
                break;
            }

            var (extractedCodeBlock, language) = ExtractFromMarkdownCodeBlock(remainingDiffContent);
            var remainingContentAfterCodeBlock = remainingDiffContent;

            if (!string.IsNullOrWhiteSpace(extractedCodeBlock))
            {
                remainingContentAfterCodeBlock = RemoveExtractedCodeBlock(remainingDiffContent);
            }

            var diffResult = CreateDiffResult(extractedCodeBlock, filePath, actionType, language);
            diffResults.Add(diffResult);

            diffContent = remainingContentAfterCodeBlock;
        }

        return diffResults;
    }

    private DiffResult CreateDiffResult(string diffContent, string? filePath, ActionType? actionType, string language)
    {
        var noneExistPath = "/dev/null";
        if (filePath == null || actionType == null)
        {
            return new DiffResult
            {
                ModifiedLines = null,
                ModifiedPath = noneExistPath,
                OriginalPath = noneExistPath,
                Language = language,
            };
        }

        if (actionType == ActionType.Add)
        {
            return new DiffResult
            {
                OriginalPath = noneExistPath,
                ModifiedPath = filePath,
                Action = ActionType.Add,
                Language = language,
                ModifiedLines = diffContent.Split('\n').ToList(), // Use '\n' for splitting
            };
        }

        if (actionType == ActionType.Delete)
        {
            return new DiffResult
            {
                OriginalPath = filePath,
                ModifiedPath = noneExistPath,
                Action = ActionType.Delete,
                Language = language,
                ModifiedLines = null,
            };
        }

        return new DiffResult
        {
            OriginalPath = filePath,
            ModifiedPath = filePath,
            Action = ActionType.Update,
            Language = language,
            ModifiedLines = diffContent.Split('\n').ToList(), // Use '\n' for splitting
        };
    }

    private static (string? filePath, ActionType? action, string remainingDiffContent) ExtractPathAndAction(
        string diffContent
    )
    {
        // Regex to match the action (Update/Add/Delete) followed by a file path
        const string pattern = @"(?<=^|\n)(Add|Update|Delete):\s*(.+)";
        var match = System.Text.RegularExpressions.Regex.Match(
            diffContent,
            pattern,
            System.Text.RegularExpressions.RegexOptions.Multiline
        );

        if (match.Success)
        {
            // Extract action and file path
            string actionType = match.Groups[1].Value;
            string filePath = match.Groups[2].Value;

            ActionType? action = actionType switch
            {
                "Add" => ActionType.Add,
                "Update" => ActionType.Update,
                "Delete" => ActionType.Delete,
                _ => null,
            };

            // Get the remaining content after the match
            int matchEndIndex = match.Index + match.Length;
            string remainingDiffContent = diffContent.Substring(matchEndIndex).Trim();

            return (filePath.Trim(), action, remainingDiffContent);
        }

        return (null, null, diffContent);
    }

    private static (string content, string language) ExtractFromMarkdownCodeBlock(string text)
    {
        // Regex to match Markdown code block with optional language
        const string pattern = @"```(\w+)?\s*([\s\S]*?)\s*```";
        var match = System.Text.RegularExpressions.Regex.Match(text, pattern);

        if (match.Success)
        {
            string language = match.Groups[1].Value;
            string content = match.Groups[2].Value;
            return (content.Trim(), language.Trim());
        }

        return (string.Empty, string.Empty);
    }

    private static string RemoveExtractedCodeBlock(string text)
    {
        // Regex to match a Markdown code block with optional language specifier
        const string pattern = @"```(?:\w+)?\s*[\s\S]*?```";
        var match = System.Text.RegularExpressions.Regex.Match(text, pattern);

        if (match.Success)
        {
            // Remove the matched code block from the text
            return text.Substring(match.Index + match.Length).Trim();
        }

        return text;
    }
}
