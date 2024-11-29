using System.ComponentModel;
using Spectre.Console.Cli;

namespace AIAssist.Commands;

[Description("Generate some settings and configs for the AI Assist.")]
public class GenerateCommand : AsyncCommand<GenerateCommand.Settings>
{
    [CommandOption("-c|--config")]
    [Description("[grey] generate base config file for the AIAssist.[/].")]
    public bool GenerateConfig { get; set; }

    [CommandOption("-i|--ignore")]
    [Description("[grey] generate AIAssist ignore file.[/].")]
    public bool GenerateIgnore { get; set; }

    public sealed class Settings : CommandSettings { }

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        return null;
    }
}
