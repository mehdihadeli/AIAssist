using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AIAssistant.Commands;

[Description("Provide interpretation and explaination for our code.")]
public class CodeInterpreterCommand : Command<CodeInterpreterCommand.Settings>
{
    public CodeInterpreterCommand() { }

    public sealed class Settings : CommandSettings
    {
        [CommandOption("-m|--model <ChatModel>")]
        [Description("[grey]llm model for chatting with ai.[/].")]
        [DefaultValue("ollama")]
        public string Model { get; set; } = default!;

        [CommandOption("-c|--context-path")]
        [Description(
            "Provide code assistance or enhance existing code or add some new features to our application context."
        )]
        [DefaultValue("")]
        public string ContextPath { get; set; } = default!;
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("[green]Interpretation process activated![/]");

        return 0;
    }
}
