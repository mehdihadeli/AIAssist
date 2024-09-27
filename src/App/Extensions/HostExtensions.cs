using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AIRefactorAssistant.Extensions;

public static class HostExtensions
{
    public static async ValueTask ExecuteConsoleRunner(this IHost host)
    {
        var runners = host.Services.GetServices<ConsoleRunner>().ToList();
        if (runners.Count != 0 == false)
            throw new Exception(
                "Console runner not found, create a console runner with implementing 'IConsoleRunner' interface"
            );

        if (runners.Count > 1)
            throw new Exception("Console app should have just one runner.");

        var runner = runners.First();

        await runner.ExecuteAsync();
    }
}
