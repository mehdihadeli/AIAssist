using AIAssist.Extensions;

namespace AIAssistant.IntegrationTests;

using Microsoft.Extensions.Hosting;

public class ApplicationFixture : IAsyncLifetime
{
    public IHost App { get; private set; } = default!;

    public Task InitializeAsync()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.AddDefaultConfigurations(); // Reusing the extension method to register configurations
        builder.AddDependencies(); // Reusing the extension method to register services
        App = builder.Build();

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        App.Dispose();
        return Task.CompletedTask;
    }
}
