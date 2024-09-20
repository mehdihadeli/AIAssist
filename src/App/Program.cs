using System.Net.Http.Headers;
using AIRefactorAssistant;
using AIRefactorAssistant.Services;
using Clients;
using Clients.Olama;
using Clients.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Spectre.Console;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Seq(serverUrl: Environment.GetEnvironmentVariable("SEQ_ADDRESS") ?? "http://localhost:5341")
    .WriteTo.Console()
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);

// Set up HttpClient for OpenAI and LlamaService based on environment
builder.Services.AddHttpClient();

var arguments = ArgumentParser.Parse(args);

//var args = Environment.GetCommandLineArgs();
string selectedModel = arguments.GetValueOrDefault("model", "ollama");
string root = arguments.GetValueOrDefault("root", string.Empty);

if (selectedModel == "openai")
{
    var openAiApiKey =
        Environment.GetEnvironmentVariable("OPENAI_API_KEY")
        ?? throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set.");
    var openAiBaseAddress =
        Environment.GetEnvironmentVariable("OPENAI_BASE_ADDRESS")
        ?? throw new InvalidOperationException("OPENAI_BASE_ADDRESS environment variable is not set.");

    builder.Services.AddHttpClient<ILanguageModelService, OpenAiService>(client =>
    {
        client.BaseAddress = new Uri(openAiBaseAddress);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);
    });
}
else
{
    var llamaApiBaseAddress =
        Environment.GetEnvironmentVariable("LLAMA_BASE_ADDRESS")
        ?? throw new InvalidOperationException("LLAMA_BASE_ADDRESS environment variable is not set.");

    builder.Services.AddHttpClient<ILanguageModelService, LlamaService>(client =>
    {
        client.BaseAddress = new Uri(llamaApiBaseAddress);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    });
}

builder.Services.AddSingleton<LLMManager>();
builder.Services.AddSingleton<CodeLoaderService>();
builder.Services.AddSingleton<EmbeddingService>();
builder.Services.AddSingleton<CodeRefactorService>();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddSerilog(dispose: true);
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var llmManager = app.Services.GetRequiredService<LLMManager>();

logger.LogInformation("Application started");
AnsiConsole.MarkupLine("[bold green]Welcome to the Code Refactor Assistant![/]");

// Initialize embeddings with code from the specified path
await llmManager.InitializeEmbeddingsAsync(root);

while (true)
{
    var userInput = AnsiConsole.Ask<string>("What would you like to do?");

    // Process user input and get suggested changes
    var completion = await llmManager.ProcessUserRequestAsync(userInput);
    AnsiConsole.MarkupLine("[bold yellow]Suggested changes:[/]");
    AnsiConsole.MarkupLine(completion);

    // Confirm with the user whether to apply the changes
    if (AnsiConsole.Confirm("Do you want to apply these changes?"))
    {
        llmManager.ApplyChangesToCodeBase(completion);
        AnsiConsole.MarkupLine("[bold green]Changes applied successfully![/]");
    }

    // Ask the user if they want to continue
    if (!AnsiConsole.Confirm("Do you want to continue?"))
    {
        logger.LogInformation("User chose to exit the application.");
        break;
    }
}

await app.RunAsync();
