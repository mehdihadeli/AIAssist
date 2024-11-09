using System.Text;

namespace AIAssistant.Prompts;

public static class SharedPrompts
{
    public const string CodeBlockFormatIsNotCorrect = "Your `code block format` is  incorrect!";

    public static string FilesAddedToChat(IEnumerable<string> fullFileContents)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("I added below files content to the context, now can you can use them for your response:");
        sb.AppendLine(Environment.NewLine);

        foreach (var fileContent in fullFileContents)
        {
            sb.AppendLine(fileContent);
            sb.AppendLine(Environment.NewLine);
        }

        return sb.ToString();
    }
}
