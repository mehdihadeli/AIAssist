using BuildingBlocks.SpectreConsole.Contracts;
using BuildingBlocks.SpectreConsole.StyleElements;
using Spectre.Console;

namespace BuildingBlocks.SpectreConsole;

public class SpectreConsoleUtilities(ColorTheme theme, IAnsiConsole console) : ISpectreConsoleUtilities
{
    public bool ConfirmationPrompt(string message)
    {
        var confirmation = console.Prompt(
            new TextPrompt<bool>(message)
                .AddChoice(true) // Corresponds to "Yes"
                .AddChoice(false) // Corresponds to "No"
                .DefaultValue(false) // Default choice
                .WithConverter(choice => choice ? "y" : "n") // Convert choice to "y" or "n"
                .PromptStyle(CreateStyle(theme.ConsoleStyle.Confirmation))
        );

        return confirmation;
    }

    public string UserPrompt(string promptMessage)
    {
        string input;
        while (true)
        {
            input = console.Prompt(
                new TextPrompt<string>(promptMessage).PromptStyle(CreateStyle(theme.ConsoleStyle.Prompt))
            );

            // Check if the input is not null or empty
            if (!string.IsNullOrWhiteSpace(input))
            {
                break;
            }

            ErrorText("Invalid input, please try again.");
        }

        return input;
    }

    public void InformationText(string message)
    {
        console.MarkupLine($"[#{CreateStringStyle(theme.ConsoleStyle.Information)}]{message}[/]");
    }

    public void Text(string message)
    {
        console.MarkupLine($"[#{CreateStringStyle(theme.ConsoleStyle.Text)}]{message}[/]");
    }

    public void ErrorText(string message)
    {
        console.MarkupLine($"[#{CreateStringStyle(theme.ConsoleStyle.Error)}]{message}[/]");
    }

    public void SuccessText(string message)
    {
        console.MarkupLine($"[#{CreateStringStyle(theme.ConsoleStyle.Success)}]{message}[/]");
    }

    public void Exception(string errorMessage, Exception ex)
    {
        ErrorText(errorMessage);
        AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
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
