using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AIAssist.Commands;

[Description("Provide tree structure of our application context.")]
public sealed class TreeStructureCommand : Command<TreeStructureCommand.Settings>
{
    public sealed class Settings : CommandSettings;

    public override int Execute(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("[green]TreeStructure process activated![/]");

        return 0;
    }
}
