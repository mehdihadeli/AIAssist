using System.ComponentModel;
using System.Text;
using AIAssistant.Contracts;
using AIAssistant.Models;
using AIAssistant.Models.Options;
using Clients;
using Clients.Chat.Models;
using Clients.Options;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AIAssistant.Commands;

[Description("Provide code assistance or enhance existing code or add some new features to our application context.")]
public class CodeAssistCommand(
    ICodeAssistantManager codeAssistantManager,
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
        [Description("[grey] code context, and the working directory relative to executing command root path.[/].")]
        public string? ContextWorkingDirectory { get; set; }

        [CommandOption("--disable-auto-context")]
        [Description("[grey] disable auto adding all files to the context.[/].")]
        public bool DisableAutoContext { get; set; }

        [CommandOption("-m|--chat-model <Chat-Model>")]
        [Description("[grey] llm model for chatting with ai. for example llama3.1.[/].")]
        [DefaultValue(ClientsConstants.Ollama.ChatModels.Deepseek_Coder_V2)]
        public string? ChatModel { get; set; }

        [CommandOption("-d|--diff <Diff-Strategy>")]
        [Description("[grey] the diff tool for showing changes. it can be `unifieddiff` or `filesnippeddiff`.[/].")]
        [DefaultValue(DiffType.FileSnippedDiff)]
        public DiffType Diff { get; set; }

        [CommandOption("-t|--code-assist-type <DiffTool>")]
        [Description("[grey] the type of code assist. it can be `embedding` or `summary`.[/].")]
        [DefaultValue(CodeAssistStrategyType.Embedding)]
        public CodeAssistStrategyType CodeAssistType { get; set; }

        [CommandOption("-e|--embedding-model <Embedding-Chat-Model>")]
        [Description("[grey] llm model for embedding purpose. for example llama3.1.[/].")]
        [DefaultValue(ClientsConstants.Ollama.EmbeddingsModels.Mxbai_Embed_Large)]
        public string? EmbeddingModel { get; set; }

        [CommandOption("-f|--files <Files>")]
        [Description("[grey] the list of files to add the context.[/].")]
        public IEnumerable<string>? Files { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("[grey]code assist mode is activated![/]");

        AnsiConsole.MarkupLine("[grey]Please Ctrl+C to exit from code assistant mode.[/]");

        // Handle Ctrl+C to exit gracefully
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            _running = false;

            AnsiConsole.MarkupLine("[red]process interrupted. Exiting...[/]");

            eventArgs.Cancel = true; // Prevent immediate termination
        };

        if (!string.IsNullOrEmpty(settings.ChatModel))
        {
            _llmOptions.ChatModel = settings.ChatModel;
        }

        if (!string.IsNullOrEmpty(settings.EmbeddingModel))
        {
            _llmOptions.EmbeddingsModel = settings.EmbeddingModel;
        }

        _options.ContextWorkingDirectory = !string.IsNullOrEmpty(settings.ContextWorkingDirectory)
            ? Path.Combine(Directory.GetCurrentDirectory(), settings.ContextWorkingDirectory)
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
            case DiffType.UnifiedDiff:
                _options.DiffType = DiffType.UnifiedDiff;
                break;
            case DiffType.FileSnippedDiff:
                _options.DiffType = DiffType.FileSnippedDiff;
                break;
        }

        switch (settings.CodeAssistType)
        {
            case CodeAssistStrategyType.Embedding:
                _options.CodeAssistType = CodeAssistStrategyType.Embedding;
                break;
            case CodeAssistStrategyType.Summary:
                _options.CodeAssistType = CodeAssistStrategyType.Summary;
                break;
        }

        var session = new ChatSession();
        await codeAssistantManager.LoadCodeFiles(session, _options.ContextWorkingDirectory, _options.Files);

        // Run in a loop until Ctrl+C is pressed
        while (_running)
        {
            //var userRequest = ReadInput("Please enter your request to apply on your code base:");

            var userRequest = "can you remove all comments in Add.cs file?";

            // Show a spinner while processing the request
            await AnsiConsole
                .Status()
                .StartAsync(
                    "Processing your request...",
                    async ctx =>
                    {
                        ctx.Spinner(Spinner.Known.Dots);
                        ctx.SpinnerStyle(Style.Parse("green"));

                        var responseStreams = codeAssistantManager.QueryAsync(userRequest);
                        var responseContent = await CollectAndWriteStreamResponseAsync(responseStreams);

                        var changesCodeBlocks = codeAssistantManager.ParseResponseCodeBlocks(responseContent);

                        foreach (var changesCodeBlock in changesCodeBlocks)
                        {
                            var confirmation = AnsiConsole.Prompt(
                                new TextPrompt<bool>($"Do you accept the changes for `{changesCodeBlock.FilePath}`?")
                                    .AddChoice(true)
                                    .AddChoice(false)
                                    .DefaultValue(true)
                                    .WithConverter(choice => choice ? "y" : "n")
                            );

                            if (confirmation)
                            {
                                codeAssistantManager.ApplyChangesToFiles([changesCodeBlock]);
                            }
                        }
                    }
                );

            // Output result after processing
            AnsiConsole.MarkupLine($"[green]Request '{userRequest}' processed successfully![/]");
            Thread.Sleep(1000); // Delay before asking again
        }

        return 0;
    }

    public async Task<string> CollectAndWriteStreamResponseAsync(IAsyncEnumerable<string?> responseStreams)
    {
        var completeResponse = new StringBuilder();

        // Collect and display each line from the response stream
        await foreach (var responseStream in responseStreams)
        {
            if (responseStream != null)
            {
                if (string.IsNullOrEmpty(responseStream))
                    continue;

                AnsiConsole.WriteLine(responseStream);
                completeResponse.Append(responseStream);
            }
        }

        // Return the complete collected response
        return completeResponse.ToString();
    }

    static string ReadInput(string prompt)
    {
        string input;
        while (true)
        {
            input = AnsiConsole.Prompt(new TextPrompt<string>(prompt));

            // Check if the input is not null or empty
            if (!string.IsNullOrWhiteSpace(input))
            {
                break; // Exit the loop when a valid input is given
            }
            AnsiConsole.WriteLine("Invalid input, please try again.");
        }
        return input;
    }
}
