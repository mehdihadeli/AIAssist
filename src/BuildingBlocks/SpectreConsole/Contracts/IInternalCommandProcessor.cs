using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.SpectreConsole.Contracts;

public interface IInternalCommandProcessor
{
    void AddCommands(IList<IInternalConsoleCommand> commands);
    Task<bool> ProcessCommand(string input, IServiceScope serviceScope);
}
