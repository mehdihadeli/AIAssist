using AIRefactorAssistant.Commands;
using AIRefactorAssistant.Options;
using AIRefactorAssistant.Services;
using BuildingBlocks.SpectreConsole;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Help;

namespace AIRefactorAssistant;

public class ConsoleRunner(
    string[] args,
    IServiceProvider serviceProvider,
    IHostApplicationLifetime appLifetime,
    IHostEnvironment hostEnvironment,
    ILogger<ConsoleRunner> logger
)
{
    public async Task ExecuteAsync()
    {
        appLifetime.ApplicationStopped.Register(() =>
        {
            Console.WriteLine("Goodbye.");
            logger.LogInformation("GoodBye");
        });

        logger.LogInformation("Application started");

        var registrar = new CustomTypeRegistrar(serviceProvider);
        var app = new CommandApp<AIAssistCommand>(registrar).WithDescription(
            "Ai Code assistant to help in writing the code."
        );

        app.Configure(config =>
        {
            // config.PropagateExceptions();

            config.SetApplicationName("aiassist");

            config.AddCommand<ChatAssistCommand>("chat").WithDescription("Chat command to send a message.");

            config.AddCommand<TreeStructureCommand>("tree").WithExample(["tree"]);

            config
                .AddCommand<CodeAssistCommand>("code")
                .WithExample(["code"])
                .WithExample(["code --model ollama"])
                .WithExample(["code --model ollama --context-path Bin/TestApp"]);

            config
                .AddCommand<CodeInterpreterCommand>("interpret")
                .WithExample(["interpret"])
                .WithExample(["interpret --model ollama"])
                .WithExample(["interpret --model ollama --context-path Bin/TestApp"]);

            config.Settings.HelpProviderStyles = new HelpProviderStyle
            {
                Description = new DescriptionStyle { Header = "bold" },
            };
        });

        //
        // var logger = app.Services.GetRequiredService<ILogger<Program>>();
        // var llmManager = app.Services.GetRequiredService<CodeRAGService>();
        //
        // logger.LogInformation("Application started");
        // AnsiConsole.MarkupLine("[bold green]Welcome to the Code Refactor Assistant![/]");
        //
        // // Initialize embeddings with code from the specified path
        // await llmManager.InitializeEmbeddingsAsync(root);
        //
        // while (true)
        // {
        //     var userInput = AnsiConsole.Ask<string>("What would you like to do?");
        //
        //     // Process user input and get suggested changes
        //     var completion = await llmManager.ProcessUserRequestAsync(userInput);
        //     AnsiConsole.MarkupLine("[bold yellow]Suggested changes:[/]");
        //     AnsiConsole.MarkupLine(completion);
        //
        //     // Confirm with the user whether to apply the changes
        //     if (AnsiConsole.Confirm("Do you want to apply these changes?"))
        //     {
        //         llmManager.ApplyChangesToCodeBase(completion);
        //         AnsiConsole.MarkupLine("[bold green]Changes applied successfully![/]");
        //     }
        //
        //     // Ask the user if they want to continue
        //     if (!AnsiConsole.Confirm("Do you want to continue?"))
        //     {
        //         logger.LogInformation("User chose to exit the application.");
        //         break;
        //     }
        // }


        try
        {
            if (args.Length == 0)
            {
                args = ["-h"];
            }
            // args should be valid for working correctly
            await app.RunAsync(args);
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(
                ex,
                hostEnvironment.IsDevelopment() ? ExceptionFormats.ShortenEverything : ExceptionFormats.ShortenTypes
            );
        }
    }
}
