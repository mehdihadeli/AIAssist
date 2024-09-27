using System.ComponentModel;
using AIRefactorAssistant.Options;
using AIRefactorAssistant.Services;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AIRefactorAssistant.Commands;

[Description("Provide code assistance or enhance existing code or add some new features to our application context.")]
public class CodeAssistCommand(CodeRAGService codeRagService, IOptions<AppOptions> options)
    : AsyncCommand<CodeAssistCommand.Settings>
{
    private readonly AppOptions _options = options.Value;
    private static bool _running = true;

    public sealed class Settings : CommandSettings
    {
        [CommandOption("-c|--context-path")]
        [Description(
            "Provide code assistance or enhance existing code or add some new features to our application context."
        )]
        [DefaultValue("")]
        public string ContextPath { get; set; } = default!;

        [CommandOption("-m|--model <Model>")]
        [Description("[grey]llm model for chatting with ai.[/].")]
        [DefaultValue("ollama")]
        public string Model { get; set; } = default!;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("[green]CodeAssistant process activated![/]");

        // Handle Ctrl+C to exit gracefully
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            _running = false;

            AnsiConsole.MarkupLine("[red]Process interrupted. Exiting...[/]");

            eventArgs.Cancel = true; // Prevent immediate termination
        };

        var path = string.IsNullOrEmpty(settings.ContextPath) ? _options.RootPath : settings.ContextPath;

        // Run in a loop until Ctrl+C is pressed
        while (_running)
        {
            var userRequest = AnsiConsole.Prompt(
                new TextPrompt<string>("Please enter your request to apply on your code base:").PromptStyle("yellow")
            );

            string response = string.Empty;

            // Show a spinner while processing the request
            await AnsiConsole
                .Status()
                .StartAsync(
                    "Processing your request...",
                    async ctx =>
                    {
                        ctx.Spinner(Spinner.Known.Dots);
                        ctx.SpinnerStyle(Style.Parse("green"));

                        // Initialize embeddings with code from the specified path
                        response = await codeRagService.ProcessUserRequestAsync(path, userRequest);
                    }
                );

            // Replacing mechanism!!

            // Output result after processing
            AnsiConsole.MarkupLine($"[green]Request '{userRequest}' processed successfully![/]");

            // Optional: Delay before repeating (could be removed if not needed)
            AnsiConsole.MarkupLine("[grey](Waiting for next request...)[/]");
            Thread.Sleep(1000); // Delay before asking again
        }

        return 0;
    }
}
