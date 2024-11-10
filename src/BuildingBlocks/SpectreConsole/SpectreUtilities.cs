using BuildingBlocks.SpectreConsole.Contracts;
using BuildingBlocks.SpectreConsole.StyleElements;
using Spectre.Console;

namespace BuildingBlocks.SpectreConsole;

public class SpectreUtilities(ColorTheme theme, IAnsiConsole console) : ISpectreUtilities
{
    public bool ConfirmationPrompt(string message)
    {
        var confirmation = console.Prompt(
            new TextPrompt<bool>(message)
                .PromptStyle(CreateStyle(theme.ConsoleStyle.Confirmation))
                .AddChoice(true) // Corresponds to "Yes"
                .AddChoice(false) // Corresponds to "No"
                .DefaultValue(false) // Default choice
                .WithConverter(choice => choice ? "y" : "n") // Convert choice to "y" or "n"
        );

        return confirmation;
    }

    public string? UserPrompt(string? promptMessage = null)
    {
        var input = string.IsNullOrEmpty(promptMessage)
            ? Console.ReadLine()
            : console.Prompt(new TextPrompt<string>(promptMessage).PromptStyle(CreateStyle(theme.ConsoleStyle.Prompt)));

        return input;
    }

    public void InformationText(string message, Justify? justify = null, Overflow? overflow = null)
    {
        console.Write(
            new Markup($"[{CreateStringStyle(theme.ConsoleStyle.Information)}]{message}[/]" + Environment.NewLine)
            {
                Overflow = overflow,
                Justification = justify,
            }
        );
    }

    public void WarningText(string message, Justify? justify = null, Overflow? overflow = null)
    {
        console.Write(
            new Markup($"[{CreateStringStyle(theme.ConsoleStyle.Warning)}]{message}[/]" + Environment.NewLine)
            {
                Overflow = overflow,
                Justification = justify,
            }
        );
    }

    public void NormalText(string message, Justify? justify = null, Overflow? overflow = null)
    {
        console.Write(
            new Markup($"[{CreateStringStyle(theme.ConsoleStyle.Text)}]{message}[/]" + Environment.NewLine)
            {
                Overflow = overflow,
                Justification = justify,
            }
        );
    }

    public void ErrorText(string message, Justify? justify = null, Overflow? overflow = null)
    {
        console.Write(
            new Markup($"[{CreateStringStyle(theme.ConsoleStyle.Error)}]{message}[/]" + Environment.NewLine)
            {
                Overflow = overflow,
                Justification = justify,
            }
        );
    }

    public void SuccessText(string message, Justify? justify = null, Overflow? overflow = null)
    {
        console.Write(
            new Markup($"[{CreateStringStyle(theme.ConsoleStyle.Success)}]{message}[/]" + Environment.NewLine)
            {
                Overflow = overflow,
                Justification = justify,
            }
        );
    }

    public void WriteCursor()
    {
        console.Markup($"[{CreateStringStyle(theme.ConsoleStyle.Cursor)}]> [/]");
    }

    public void WriteRule()
    {
        console.Write(new Rule());
    }

    public void Exception(string errorMessage, Exception ex)
    {
        ErrorText(errorMessage);
        console.WriteException(ex, ExceptionFormats.ShortenEverything);
    }

    public void DirectoryTree(string path, int indentLevel)
    {
        var directories = Directory.GetDirectories(path);
        var files = Directory.GetFiles(path);

        var indent = new string(' ', indentLevel * 4);

        console.MarkupLine($"{indent}[{CreateStringStyle(theme.ConsoleStyle.Tree)}]{Path.GetFileName(path)}[/]"); // Bold the directory name

        // Print each file in the current directory
        foreach (var file in files)
        {
            console.MarkupLine(
                $"{indent}  └── [{CreateStringStyle(theme.ConsoleStyle.Tree)}]{Path.GetFileName(file)}[/]"
            ); // Cyan for files
        }

        // Recursively print each subdirectory
        foreach (var directory in directories)
        {
            DirectoryTree(directory, indentLevel + 1);
        }
    }

    public IEnumerable<string> GetArguments(string input)
    {
        // Get the arguments after the command
        int spaceIndex = input.IndexOf(' ', StringComparison.Ordinal);
        var elements = spaceIndex != -1 ? input.Substring(spaceIndex + 1).Trim() : string.Empty;

        var arguments = elements
            .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToList();

        return arguments;
    }

    public bool PressedShortcutKey(
        IList<IInternalConsoleCommand> commands,
        ConsoleModifiers consoleModifierKey,
        out string pressedKey
    )
    {
        var keyInfo = Console.ReadKey(intercept: true);

        if (
            keyInfo.Modifiers.HasFlag(consoleModifierKey)
            && commands.Any(x => x.ShortcutKey is not null && x.ShortcutKey == keyInfo.Key)
        )
        {
            var userInput = commands
                .SingleOrDefault(x => x.ShortcutKey is not null && x.ShortcutKey == keyInfo.Key)
                ?.Name;

            if (!string.IsNullOrEmpty(userInput))
            {
                pressedKey = userInput;
                return true;
            }
        }

        pressedKey = keyInfo.KeyChar.ToString();

        return false;
    }

    public void Clear()
    {
        console.Clear();
    }

    private Style CreateStyle(StyleBase styleBase)
    {
        var style = Style.Parse(CreateStringStyle(styleBase));

        return style;
    }

    private string CreateStringStyle(StyleBase styleBase)
    {
        var italic = styleBase.Italic ? "italic" : "default";
        var bold = styleBase.Bold ? "bold" : "default";
        var underline = styleBase.Underline ? "underline" : "default";

        var style =
            $"{
            styleBase.Foreground ?? "default"
        } on {
            styleBase.Background ?? "default"
        } {
            italic
        } {
            bold
        } {
            underline
        }";

        return style;
    }
}
