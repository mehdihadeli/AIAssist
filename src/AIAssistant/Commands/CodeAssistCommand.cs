using System.ComponentModel;
using AIAssistant.Models;
using AIAssistant.Options;
using AIAssistant.Services;
using Clients;
using Clients.Models;
using Clients.Options;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AIAssistant.Commands;

[Description("Provide code assistance or enhance existing code or add some new features to our application context.")]
public class CodeAssistCommand(
    CodeRAGService codeRagService,
    IOptions<CodeAssistOptions> options,
    IOptions<LLMOptions> llmOptions
) : AsyncCommand<CodeAssistCommand.Settings>
{
    private readonly CodeAssistOptions _options = options.Value;
    private readonly LLMOptions _llmOptions = llmOptions.Value;
    private static bool _running = true;

    public sealed class Settings : CommandSettings
    {
        [CommandOption("-c|--context-path")]
        [Description("[grey] code context, and the working directory relative for the assistant.[/].")]
        public string? ContextPath { get; set; }

        [CommandOption("--disable-auto-context")]
        [Description("[grey] disable auto adding all files to the context.[/].")]
        public bool DisableAutoContext { get; set; }

        [CommandOption("-m|--chat-model <Chat-Model>")]
        [Description("[grey] llm model for chatting with ai. for example llama3.1.[/].")]
        [DefaultValue(Constants.Ollama.ChatModels.Llama3_1)]
        public string? ChatModel { get; set; }

        [CommandOption("-d|--diff-tool <DiffTool>")]
        [Description("[grey] the diff tool for showing changes. it can be `gitdiff` or `codediff`.[/].")]
        [DefaultValue(DiffType.CodeDiff)]
        public DiffType Diff { get; set; }

        [CommandOption("-e|--embedding-model <Embedding-Chat-Model>")]
        [Description("[grey] llm model for embedding purpose. for example llama3.1.[/].")]
        [DefaultValue(Constants.Ollama.EmbeddingsModels.Mxbai_Embed_Large)]
        public string? EmbeddingModel { get; set; }

        [CommandOption("-p|--provider <AI-Provider>")]
        [Description("[grey] ai provider to use. for example openai, ollama or anthropic.[/].")]
        [DefaultValue(AIProvider.Ollama)]
        public AIProvider AIProvider { get; set; }

        [CommandOption("-f|--files <Files>")]
        [Description("[grey] the list of files to add the context.[/].")]
        public IEnumerable<string>? Files { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("[grey]code assist mode is activated![/]");

        AnsiConsole.MarkupLine("[grey]Please Ctrl+C to exit from code assistant mode.[/]");

        // // Handle Ctrl+C to exit gracefully
        // Console.CancelKeyPress += (sender, eventArgs) =>
        // {
        //     _running = false;
        //
        //     AnsiConsole.MarkupLine("[red]process interrupted. Exiting...[/]");
        //
        //     eventArgs.Cancel = true; // Prevent immediate termination
        // };
        //
        try
        {
            switch (settings.AIProvider)
            {
                case AIProvider.Ollama:
                    _llmOptions.ProviderType = AIProvider.Ollama;
                    break;
                case AIProvider.OpenAI:
                    _llmOptions.ProviderType = AIProvider.OpenAI;
                    break;
                case AIProvider.Anthropic:
                    _llmOptions.ProviderType = AIProvider.Anthropic;
                    break;
                default:
                    _llmOptions.ProviderType = AIProvider.Ollama;
                    break;
            }

            if (!string.IsNullOrEmpty(settings.ChatModel))
            {
                _llmOptions.ChatModel = settings.ChatModel;
            }

            if (!string.IsNullOrEmpty(settings.EmbeddingModel))
            {
                _llmOptions.EmbeddingsModel = settings.EmbeddingModel;
            }

            _options.ContextWorkingDirectory = !string.IsNullOrEmpty(settings.ContextPath)
                ? Path.Combine(Directory.GetCurrentDirectory(), settings.ContextPath)
                : Directory.GetCurrentDirectory(); // set to current working directory

            if (settings.Files is not null && settings.Files.Any())
            {
                _options.Files = settings.Files;
            }

            if (settings.DisableAutoContext)
            {
                _options.AutoContextEnabled = false;
            }

            switch (settings.Diff)
            {
                case DiffType.GitDiff:
                    _options.DiffType = DiffType.GitDiff;
                    break;
                case DiffType.CodeDiff:
                    _options.DiffType = DiffType.CodeDiff;
                    break;
                default:
                    _options.DiffType = DiffType.GitDiff;
                    break;
            }

            var session = new ChatSession();
            codeRagService.Initialize(session).GetAwaiter().GetResult();

            Console.WriteLine("Please enter your request to apply on your code base:");
            var userRequest = Console.ReadLine();

            // Run in a loop until Ctrl+C is pressed
            while (_running)
            {
                // var userRequest = AnsiConsole.Prompt(
                //     new TextPrompt<string>("Please enter your request to apply on your code base:")
                // );
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
                            var codeChanges = await codeRagService.ModifyOrAddCodeAsync(session, userRequest);
                        }
                    );

                // Output result after processing
                AnsiConsole.MarkupLine($"[green]Request '{userRequest}' processed successfully![/]");

                // Optional: Delay before repeating (could be removed if not needed)
                AnsiConsole.MarkupLine("[grey](Waiting for next request...)[/]");
                Thread.Sleep(1000); // Delay before asking again
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return 0;
    }
}
