using AIRefactorAssistant;
using AIRefactorAssistant.Commands;
using AIRefactorAssistant.Data;
using AIRefactorAssistant.Extensions;
using AIRefactorAssistant.Extensions.HostApplicationBuilderExtensions;
using AIRefactorAssistant.Options;
using AIRefactorAssistant.Services;
using BuildingBlocks.Core.HostApplicationBuilderExtensions;
using BuildingBlocks.InMemoryVectorDatabase;
using Clients.Anthropic;
using Clients.Ollama;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

// https://github.com/serilog/serilog-aspnetcore#two-stage-initialization
// https://github.com/serilog/serilog-extensions-hosting
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level} - {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

var builder = Host.CreateApplicationBuilder(args);

builder.AddConfigurationOptions<AppOptions>(nameof(AppOptions));
builder.AddConfigurationOptions<LogOptions>(nameof(LogOptions));
builder.AddConfigurationOptions<AnthropicOptions>(nameof(AnthropicOptions));
builder.AddConfigurationOptions<OllamaOptions>(nameof(OllamaOptions));

builder.AddClients();

builder.Services.AddSingleton<CodeRAGService>();
builder.Services.AddSingleton<CodeLoaderService>();
builder.Services.AddSingleton<EmbeddingService>();
builder.Services.AddSingleton<EmbeddingsStore>();
builder.Services.AddSingleton<VectorDatabase>();

builder.Services.AddSingleton<CodeAssistCommand>();
builder.Services.AddSingleton<CodeInterpreterCommand>();
builder.Services.AddSingleton<ChatAssistCommand>();
builder.Services.AddSingleton<TreeStructureCommand>();
builder.Services.AddSingleton<AIAssistCommand>();

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
    sp.GetRequiredService<IHostEnvironment>(),
    sp.GetRequiredService<ILogger<ConsoleRunner>>()
));

var app = builder.Build();

await app.ExecuteConsoleRunner();
