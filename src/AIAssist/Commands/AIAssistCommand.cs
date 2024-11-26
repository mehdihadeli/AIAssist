using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AIAssist.Commands;

// commands should be state-less after each run
public class AIAssistCommand : Command<AIAssistCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-v|--version")]
        [Description("Display the version of application.")]
        public bool Version { get; set; }

        [CommandOption("-l|--llms")]
        [Description("Display the suppurted llms.")]
        public bool LLMLists { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        while (true)
        {
            Console.WriteLine("Enter your prompt:");
            var data = Console.ReadLine();
        }
        if (settings.LLMLists)
        {
            AnsiConsole.WriteLine("LLMLists option is activated");
        }

        if (settings.Version)
        {
            AnsiConsole.WriteLine("Version is activated");
        }

        return 0;
    }
}
