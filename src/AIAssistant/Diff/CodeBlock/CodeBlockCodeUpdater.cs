using Spectre.Console;

namespace AIAssistant.Diff.CodeBlock;

public class CodeBlockCodeUpdater
{
    public void ApplyChanges(IList<CodeBlock> codeBlocks)
    {
        foreach (var codeBlock in codeBlocks)
        {
            if (string.IsNullOrWhiteSpace(codeBlock.FilePath) || string.IsNullOrWhiteSpace(codeBlock.FileContent))
                continue;

            try
            {
                // Write the full content to the specified file
                var directoryPath = Path.GetDirectoryName(codeBlock.FilePath);

                if (!string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Replace or create the file with the new content
                File.WriteAllText(codeBlock.FilePath, codeBlock.FileContent);
                AnsiConsole.MarkupLine($"[green]File updated: {codeBlock.FilePath}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Failed to update file {codeBlock.FilePath}: {ex.Message}[/]");
            }
        }
    }
}
