using BuildingBlocks.SpectreConsole.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.SpectreConsole;

public class InternalCommandProcessor : IInternalCommandProcessor
{
    private readonly IList<IInternalConsoleCommand> _commands = new List<IInternalConsoleCommand>();

    public void AddCommands(IList<IInternalConsoleCommand> commands)
    {
        foreach (var command in commands)
        {
            if (_commands.Any(x => x.Name == command.Name))
            {
                throw new Exception($"Command {command.Name} already exists in the commands list");
            }

            _commands.Add(command);
        }
    }

    public async Task<bool> ProcessCommand(string input, IServiceScope serviceScope)
    {
        // Find the command based on the user input (either by Command or ShortCommand)
        var command = _commands.FirstOrDefault(cmd =>
            (!string.IsNullOrEmpty(cmd.ShortCommand) && input.StartsWith(cmd.ShortCommand))
            || (!string.IsNullOrEmpty(cmd.Command) && input.StartsWith(cmd.Command))
        );

        if (command != null)
        {
            return await command.ExecuteAsync(serviceScope, input);
        }
        else
        {
            var defaultCommand = _commands.SingleOrDefault(x => x.IsDefaultCommand);
            if (defaultCommand is not null)
            {
                return await defaultCommand.ExecuteAsync(serviceScope, input);
            }

            return true;
        }
    }
}
