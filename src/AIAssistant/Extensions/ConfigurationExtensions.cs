using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace AIAssistant.Extensions;

public static class ConfigurationExtensions
{
    public static HostApplicationBuilder AddDefaultConfigurations(this HostApplicationBuilder builder)
    {
        // remove default .net configuration like appsettings.json
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddEnvironmentVariables();

        AddEmbeddedConfiguration(builder);

        // load `config.json` from `current working directory`
        builder.Configuration.AddJsonFile("config.json", optional: true, reloadOnChange: true);

        return builder;
    }

    private static void AddEmbeddedConfiguration(HostApplicationBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{nameof(AIAssistant)}.default-config.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream != null)
        {
            // Load the JSON configuration from the embedded resource
            var embeddedConfiguration = new ConfigurationBuilder().AddJsonStream(stream).Build();

            // Merge the embedded configuration with the existing configuration
            builder.Configuration.AddConfiguration(embeddedConfiguration);
        }
        else
        {
            Console.WriteLine("Embedded default-config.json not found.");
        }
    }
}
