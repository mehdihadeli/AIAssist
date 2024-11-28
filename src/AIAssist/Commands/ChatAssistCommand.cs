using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AIAssist.Commands;

[Description("Provide a chat assistant with ai for asking any questions.")]
public sealed class ChatAssistCommand : Command<ChatAssistCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-m|--model <ChatModel>")]
        [Description("[grey]llm model for chatting with ai.[/].")]
        [DefaultValue("ollama")]
        public string Model { get; set; } = default!;
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("[green]ChatAssistant process activated![/]");

        return 0;
    }
}
