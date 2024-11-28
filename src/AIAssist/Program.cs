using AIAssist.Commands;
using AIAssist.Extensions;
using BuildingBlocks.SpectreConsole;
using Microsoft.Extensions.Hosting;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

// because of conflict console logger with our console application we should not use Console serilog enricher
// https://github.com/serilog/serilog-aspnetcore#two-stage-initialization
// https://github.com/serilog/serilog-extensions-hosting
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level} - {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.AddDefaultConfigurations();

    // https://github.com/serilog/serilog-extensions-hosting
    // https://github.com/serilog/serilog-aspnetcore#two-stage-initialization
    // Routes framework log messages through Serilog - get other sinks from top level definition
    builder.Services.AddSerilog(
        (sp, loggerConfiguration) =>
        {
            // The downside of initializing Serilog in top level is that services from the ASP.NET Core host, including the appsettings.json configuration and dependency injection, aren't available yet.
            // setup sinks that related to `configuration` here instead of top level serilog configuration
            loggerConfiguration.ReadFrom.Configuration(builder.Configuration);
        }
    );

    builder.AddDependencies();

    var app = builder.Build();

    // https: //spectreconsole.net/cli/commandapp
    var commandApp = new CommandApp<AIAssistCommand>(new CustomTypeRegistrar(app)).WithDescription(
        "Ai Code assistant to help in writing the code."
    );

    commandApp.Configure(config =>
    {
        config.PropagateExceptions();

        config.SetApplicationName("aiassist");

        config
            .AddCommand<CodeAssistCommand>("code")
            .WithExample(["code"])
            .WithExample(["code --model ollama"])
            .WithExample(["code --model ollama --context-path Bin/TestApp"]);
    });

    if (args.Length == 0)
    {
        AnsiConsole.Write(new FigletText("AI Assist").Centered().Color(Color.Purple));
        args = ["-h"];
    }

    // args should be valid in Spectre.Console to work correctly
    await commandApp.RunAsync(args);
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
    Console.ReadKey();
}
finally
{
    Log.CloseAndFlush();
}
