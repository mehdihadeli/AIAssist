using AIRefactorAssistant;
using AIRefactorAssistant.Data;
using AIRefactorAssistant.Extensions;
using AIRefactorAssistant.Extensions.HostApplicationBuilderExtensions;
using AIRefactorAssistant.Options;
using AIRefactorAssistant.Services;
using BuildingBlocks.Core.HostApplicationBuilderExtensions;
using BuildingBlocks.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

// https://github.com/serilog/serilog-aspnetcore#two-stage-initialization
// https://github.com/serilog/serilog-extensions-hosting
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level} - {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

var builder = Host.CreateApplicationBuilder(args);

// Set up HttpClient for OpenAI and OllamaService based on environment
///builder.Services.AddHttpClient();

var arguments = ArgumentParser.Parse(args);

//var args = Environment.GetCommandLineArgs();
string selectedModel = arguments.GetValueOrDefault("model", "ollama");
string root = arguments.GetValueOrDefault("root", string.Empty);

builder.Services.AddOptions<AppOptions>().BindConfiguration(nameof(AppOptions));
builder.Services.AddOptions<LogOptions>().BindConfiguration(nameof(LogOptions));
builder.Services.AddOptions<AnthropicOptions>().BindConfiguration(nameof(AnthropicOptions));
builder.Services.AddOptions<OpenAIOptions>().BindConfiguration(nameof(OpenAIOptions));
builder.AddValidationOptions<OllamaOptions>(nameof(OllamaOptions));

builder.AddClients();

builder.Services.AddSingleton<CodeRAGService>();
builder.Services.AddSingleton<CodeLoaderService>();
builder.Services.AddSingleton<EmbeddingService>();
builder.Services.AddSingleton<CodeRefactorService>();
builder.Services.AddSingleton<EmbeddingsStore>();

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

builder.Services.AddSingleton(sp => new ConsoleRunner(
    args,
    sp,
    sp.GetRequiredService<IHostApplicationLifetime>(),
    sp.GetRequiredService<ILogger<ConsoleRunner>>()
));

var app = builder.Build();

await app.ExecuteConsoleRunner();
