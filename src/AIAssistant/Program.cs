using AIAssistant.Commands;
using AIAssistant.Extensions;
using BuildingBlocks.SpectreConsole;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

// https://github.com/serilog/serilog-aspnetcore#two-stage-initialization
// https://github.com/serilog/serilog-extensions-hosting
// because of conflict console logger with our console application we should not use Console serilog enricher

bool isDev = false;

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.AddDefaultConfigurations();

    builder.AddDependencies();

    builder.Services.AddLogging();

    isDev = builder.Environment.IsDevelopment();

    var app = builder.Build();

    // https: //spectreconsole.net/cli/commandapp
    var commandApp = new CommandApp<AIAssistCommand>(new CustomTypeRegistrar(app)).WithDescription(
        "Ai Code assistant to help in writing the code."
    );

    commandApp.Configure(config =>
    {
        config.PropagateExceptions();

        config.SetApplicationName("aiassist");

        // config.AddCommand<ChatAssistCommand>("chat").WithDescription("Chat command to send a message.");
        //
        // config.AddCommand<TreeStructureCommand>("tree").WithExample(["tree"]);

        config
            .AddCommand<CodeAssistCommand>("code")
            .WithExample(["code"])
            .WithExample(["code --model ollama"])
            .WithExample(["code --model ollama --context-path Bin/TestApp"]);

        // config
        //     .AddCommand<CodeInterpreterCommand>("explanation")
        //     .WithExample(["explanation"])
        //     .WithExample(["explanation --model ollama"])
        //     .WithExample(["explanation --model ollama --context-path Bin/TestApp"]);
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
    AnsiConsole.WriteException(ex, isDev ? ExceptionFormats.ShortenEverything : ExceptionFormats.ShortenTypes);
}
finally
{
    Log.CloseAndFlush();
}
