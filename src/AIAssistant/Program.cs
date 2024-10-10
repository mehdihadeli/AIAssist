using AIAssistant.Extensions;
using AIAssistant.Options;
using AIAssistant.Services;
using Clients.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

// https://github.com/serilog/serilog-aspnetcore#two-stage-initialization
// https://github.com/serilog/serilog-extensions-hosting
// because of conflict console logger with our console application we should not use Console serilog enricher

bool isDev = false;

AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
{
    Console.WriteLine($"Unhandled exception, {eventArgs.ExceptionObject}");
};

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.AddDependencies();

    isDev = builder.Environment.IsDevelopment();

    var app = builder.Build();

    var codeOptions = app.Services.GetRequiredService<IOptions<CodeAssistOptions>>();
    codeOptions.Value.ContextWorkingDirectory = Directory.GetCurrentDirectory();

    var session = new ChatSession();
    var codeRagService = app.Services.GetRequiredService<CodeRAGService>();
    await codeRagService.Initialize(session);

    // var e = app.Services.GetRequiredService<EmbeddingService>();
    //
    // for (int i = 0; i < 50; i++)
    // {
    //     await e.GenerateEmbeddingForUserInput(Guid.NewGuid().ToString());
    // }

    // Run in a loop until Ctrl+C is pressed
    while (true)
    {
        // var userRequest = AnsiConsole.Prompt(
        //     new TextPrompt<string>("Please enter your request to apply on your code base:")
        // );
        // Show a spinner while processing the request
        // await AnsiConsole
        //     .Status()
        //     .StartAsync(
        //         "Processing your request...",
        //         async ctx =>
        //         {
        //             ctx.Spinner(Spinner.Known.Dots);
        //             ctx.SpinnerStyle(Style.Parse("green"));
        //
        //             // Initialize embeddings with code from the specified path
        //         }
        //     );

        // Console.WriteLine("Please enter your request to apply on your code base: can you remove all comments in the classes");
        // var userRequest = Console.ReadLine();
        //
        var userRequest = "can you remove all comments in the classes";
        var codeChanges = await codeRagService.ModifyOrAddCodeAsync(session, userRequest);

        Console.WriteLine("Wait for next iteration.");
        Console.ReadKey();
        // // Output result after processing
        // AnsiConsole.MarkupLine($"[green]Request '{userRequest}' processed successfully![/]");
        //
        // // Optional: Delay before repeating (could be removed if not needed)
        // AnsiConsole.MarkupLine("[grey](Waiting for next request...)[/]");
        // Thread.Sleep(1000); // Delay before asking again
    }

    // // https: //spectreconsole.net/cli/commandapp
    // var commandApp = new CommandApp<AIAssistCommand>(new CustomTypeRegistrar(app)).WithDescription(
    //     "Ai Code assistant to help in writing the code."
    // );
    //
    // commandApp.Configure(config =>
    // {
    //     config.PropagateExceptions();
    //
    //     config.SetApplicationName("aiassist");
    //
    //     // config.AddCommand<ChatAssistCommand>("chat").WithDescription("Chat command to send a message.");
    //     //
    //     // config.AddCommand<TreeStructureCommand>("tree").WithExample(["tree"]);
    //
    //     config
    //         .AddCommand<CodeAssistCommand>("code")
    //         .WithExample(["code"])
    //         .WithExample(["code --model ollama"])
    //         .WithExample(["code --model ollama --context-path Bin/TestApp"]);
    //
    //     // config
    //     //     .AddCommand<CodeInterpreterCommand>("interpret")
    //     //     .WithExample(["interpret"])
    //     //     .WithExample(["interpret --model ollama"])
    //     //     .WithExample(["interpret --model ollama --context-path Bin/TestApp"]);
    // });
    //
    // if (args.Length == 0)
    // {
    //     AnsiConsole.Write(new FigletText("AI Assist").Centered().Color(Color.Purple));
    //     args = ["-h"];
    // }

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
    //     var completion = await llmManager.ModifyOrAddCodeAsync(userInput);
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

    // // args should be valid in Spectre.Console to work correctly
    // var s = await commandApp.RunAsync(args);
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    // AnsiConsole.WriteException(ex, isDev ? ExceptionFormats.ShortenEverything : ExceptionFormats.ShortenTypes);
}
finally
{
    Log.CloseAndFlush();
}
