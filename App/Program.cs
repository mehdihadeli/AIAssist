using System.Net.Http.Headers;
using System.Net.Http.Json;
using AIRefactorAssistant.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using Spectre.Console;
using ILogger = Microsoft.Extensions.Logging.ILogger;

List<(string releventFilePath, string chunk, string embedding)> codeEmbeddings = new();

var customAppPath = args.FirstOrDefault() ?? String.Empty;

var seqAddress =
    Environment.GetEnvironmentVariable("SEQ_ADDRESS")
    ?? throw new InvalidOperationException("SEQ_ADDRESS environment variable is not set.");

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    //.WriteTo.Seq(seqAddress)
    .CreateLogger();

var services = new ServiceCollection();

var openAiApiKey =
    Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set.");
var openAiBaseAddress =
    Environment.GetEnvironmentVariable("OPENAI_BASE_ADDRESS")
    ?? throw new InvalidOperationException("OPENAI_BASE_ADDRESS environment variable is not set.");

services.AddHttpClient(
    "OpenAI",
    client =>
    {
        client.BaseAddress = new Uri(openAiBaseAddress);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            openAiApiKey
        );
    }
);
services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddSerilog(dispose: true);
});

var serviceProvider = services.BuildServiceProvider();

var globalLogger = serviceProvider.GetRequiredService<ILogger<Program>>();
globalLogger.LogInformation("Application started");

var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
var httpClient = httpClientFactory.CreateClient("OpenAI");

AnsiConsole.MarkupLine("[bold green]Welcome to the Code Refactor Assistant![/]");
globalLogger.LogInformation("Displayed welcome message");

// Step 1: Load the application code from the specified path
LoadApplicationCode(customAppPath, codeEmbeddings);

// Step 2: Generate embeddings for the loaded code
await GenerateEmbeddingsForCode(httpClient, codeEmbeddings, globalLogger);

while (true)
{
    // Prompt the user for a question or command
    var userInput = AnsiConsole.Ask<string>(
        "What would you like to do? (e.g., [bold]Add a feature[/] or [bold]Refactor code[/]):"
    );
    globalLogger.LogInformation("User input: {userInput}", userInput);

    // Generate an embedding for the user input
    var userEmbedding = await GetEmbeddingAsync(httpClient, userInput, globalLogger);

    // Find relevant code snippets based on user query
    var relevantCode = FindMostRelevantCode(userEmbedding, codeEmbeddings);
    globalLogger.LogInformation(
        "Found {relevantCodeCount} relevant code snippets",
        relevantCode.Count
    );

    // Prepare context for the LLM by combining relevant code snippets
    var context = PrepareContextFromCode(relevantCode);

    // Generate a response from OpenAI using the context
    var completion = await GetOpenAiResponseAsync(httpClient, userInput, context, globalLogger);
    globalLogger.LogInformation("OpenAI response generated");

    // Display the suggested changes
    AnsiConsole.MarkupLine("[bold yellow]Suggested changes:[/]");
    AnsiConsole.MarkupLine(completion);

    // Confirm the changes with the user
    var confirmation = AnsiConsole.Confirm("Do you want to apply these changes?");
    globalLogger.LogInformation("User confirmation: {confirmation}", confirmation);

    if (confirmation)
    {
        // Apply changes to the codebase
        ApplyChangesToCodeBase(relevantCode, completion);
        globalLogger.LogInformation("Changes applied successfully to the codebase");
        AnsiConsole.MarkupLine("[bold green]Changes applied successfully![/]");
    }
    else
    {
        globalLogger.LogInformation("User discarded the changes");
        AnsiConsole.MarkupLine("[bold red]Changes discarded.[/]");
    }

    // Ask if the user wants to continue
    if (!AnsiConsole.Confirm("Do you want to continue?"))
    {
        globalLogger.LogInformation("User chose to exit the application");
        break;
    }
}

void LoadApplicationCode(
    string appRelativePath,
    List<(string relativeFilePath, string chunk, string embedding)> embeddings
)
{
    // Get the absolute path to the application directory
    var rootPath = Directory.GetCurrentDirectory(); // Context root
    var appPath = Path.Combine(rootPath, appRelativePath);

    globalLogger.LogInformation("AppPath is: {appPath}", appPath);

    var csFiles = Directory.GetFiles(appPath, "*.cs", SearchOption.AllDirectories);
    AnsiConsole.MarkupLine("[bold blue]Loading code files from directory...[/]");

    foreach (var file in csFiles)
    {
        // Get the relative path based on the context root
        var relativePath = Path.GetRelativePath(appPath, file);
        globalLogger.LogInformation("App relative path is: {relativePath}", relativePath);

        var fileContent = File.ReadAllText(file);
        var chunks = ChunkCode(fileContent);

        foreach (var chunk in chunks)
        {
            embeddings.Add((relativePath, chunk, string.Empty)); // We'll fill embeddings later
        }
    }

    AnsiConsole.MarkupLine("[bold blue]Code files loaded successfully![/]");
}

// Step 2: Generate embeddings for all code chunks
async Task GenerateEmbeddingsForCode(
    HttpClient client,
    List<(string fileName, string chunk, string embedding)> embeddings,
    ILogger logger
)
{
    AnsiConsole.MarkupLine("[bold yellow]Generating embeddings for code...[/]");
    logger.LogInformation("Started generating embeddings for code");

    foreach (var (fileName, chunk, _) in embeddings.ToList())
    {
        var embedding = await GetEmbeddingAsync(client, chunk, logger);
        embeddings = embeddings
            .Select(e =>
                e.fileName == fileName && e.chunk == chunk ? (fileName, chunk, embedding) : e
            )
            .ToList();
    }

    AnsiConsole.MarkupLine("[bold green]Embeddings generated![/]");
    logger.LogInformation("Embeddings generated for all code chunks");
}

// Helper: Break the code into smaller chunks (for OpenAI token limits)
List<string> ChunkCode(string code, int maxChunkSize = 1000)
{
    var chunks = new List<string>();

    for (int i = 0; i < code.Length; i += maxChunkSize)
    {
        chunks.Add(code.Substring(i, Math.Min(maxChunkSize, code.Length - i)));
    }

    return chunks;
}

// Step 4: Generate embedding for a given text (user input or code chunk)
async Task<string> GetEmbeddingAsync(HttpClient client, string text, ILogger logger)
{
    var requestBody = new { input = new[] { text }, model = "text-embedding-ada-002" };

    logger.LogInformation(
        "Sending embedding request for text: {text} and requestBody: {requestBody}",
        text,
        requestBody
    );

    var response = await client.PostAsJsonAsync("embeddings", requestBody);
    var responseContent = await response.Content.ReadAsStringAsync();

    if (response.IsSuccessStatusCode)
    {
        logger.LogInformation(
            "Received successful response for embedding. content: {responseContent}",
            responseContent
        );
    }
    else
    {
        logger.LogError("Error in embedding response: {responseContent}", responseContent);
    }

    var embedding = JsonConvert.DeserializeObject<OpenAiEmbeddingResponse>(responseContent);
    return string.Join(",", embedding?.Data.First().Embedding!);
}

// Step 5: Find the most relevant code snippets based on user query embedding
List<(string releventFilePath, string chunk)> FindMostRelevantCode(
    string userEmbedding,
    List<(string releventFilePath, string chunk, string embedding)> embeddings
)
{
    var userEmbeddingVector = userEmbedding.Split(',').Select(double.Parse).ToArray();

    var relevantCode = embeddings
        .Select(ce =>
            (
                ce.releventFilePath,
                ce.chunk,
                score: CosineSimilarity(ce.embedding, userEmbeddingVector)
            )
        )
        .OrderByDescending(ce => ce.score)
        .Take(3) // Get top 3 most similar chunks
        .Select(ce => (ce.releventFilePath, ce.chunk))
        .ToList();

    return relevantCode;
}

// Step 6: Prepare context from relevant code snippets
string PrepareContextFromCode(List<(string releventFilePath, string chunk)> relevantCode)
{
    return string.Join(
        Environment.NewLine + Environment.NewLine,
        relevantCode.Select(rc => rc.chunk)
    );
}

// Step 7: Get response from OpenAI with context and user query
async Task<string> GetOpenAiResponseAsync(
    HttpClient client,
    string prompt,
    string context,
    ILogger logger
)
{
    var requestBody = new
    {
        model = "text-davinci-003",
        prompt = $"The following is code from the application: {context}. Based on the user's input: {prompt}, suggest changes or enhancements.",
        max_tokens = 150,
        temperature = 0.5,
    };

    logger.LogInformation(
        "Sending completion request to OpenAI, requestBody: {requestBody}",
        requestBody
    );

    var response = await client.PostAsJsonAsync("completions", requestBody);
    var responseContent = await response.Content.ReadAsStringAsync();

    if (response.IsSuccessStatusCode)
    {
        logger.LogInformation(
            "Received successful response for completion, responseContent: {responseContent}",
            responseContent
        );
    }
    else
    {
        logger.LogError("Error in OpenAI completion response: {responseContent}", responseContent);
    }

    var completionResponse = JsonConvert.DeserializeObject<OpenAiCompletionResponse>(
        responseContent
    );

    return completionResponse?.Choices.First().Text.Trim()!;
}

// Helper: Calculate cosine similarity between embeddings
double CosineSimilarity(string embedding, double[] userEmbeddingVector)
{
    var embeddingVector = embedding.Split(',').Select(double.Parse).ToArray();
    double dotProduct = embeddingVector.Zip(userEmbeddingVector, (a, b) => a * b).Sum();
    double magnitudeA = Math.Sqrt(embeddingVector.Select(a => a * a).Sum());
    double magnitudeB = Math.Sqrt(userEmbeddingVector.Select(b => b * b).Sum());

    return dotProduct / (magnitudeA * magnitudeB);
}

// Apply the changes to the most relevant code files
void ApplyChangesToCodeBase(
    List<(string releventFilePath, string chunk)> relevantCode,
    string completion
)
{
    // Get the absolute path to the application directory
    var rootPath = Directory.GetCurrentDirectory(); // Context root

    foreach (var (relevantFilePath, oldChunk) in relevantCode)
    {
        var filePath = Path.Combine(rootPath, relevantFilePath);

        // Read the content of the source file
        var fileContent = File.ReadAllText(filePath);

        // Find the old chunk in the source file
        if (fileContent.Contains(oldChunk))
        {
            // Replace the old chunk with the completion
            var updatedContent = fileContent.Replace(oldChunk, completion);

            // Write the updated content back to the file
            File.WriteAllText(filePath, updatedContent);

            AnsiConsole.MarkupLine($"[bold green]Changes applied to {relevantFilePath}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine(
                $"[bold red]Could not find the code chunk in {relevantFilePath}. Changes were not applied.[/]"
            );
        }
    }
}
