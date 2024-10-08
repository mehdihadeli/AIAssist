using AIAssistant.Commands;
using AIAssistant.Contracts;
using AIAssistant.Data;
using AIAssistant.Extensions.HostApplicationBuilderExtensions;
using AIAssistant.Options;
using AIAssistant.Services;
using BuildingBlocks.Extensions;
using BuildingBlocks.InMemoryVectorDatabase;
using BuildingBlocks.Serialization;
using Clients;
using Clients.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace AIAssistant.Extensions;

public static class DependencyInjectionExtensions
{
    public static HostApplicationBuilder AddDependencies(this HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton(AnsiConsole.Console);
        builder.Services.AddSingleton<IJsonSerializer, JsonObjectSerializer>();

        builder.AddConfigurationOptions<CodeAssistOptions>(nameof(CodeAssistOptions));
        builder.AddConfigurationOptions<LogOptions>(nameof(LogOptions));
        builder.AddConfigurationOptions<LLMOptions>(nameof(LLMOptions));
        builder.AddConfigurationOptions<PolicyOptions>(nameof(PolicyOptions));

        builder.Services.AddSingleton<LLMClientFactory>();

        builder.Services.AddSingleton<ILLMServiceManager>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<LLMOptions>>().Value;

            // factory pattern
            var llmFactory = sp.GetRequiredService<LLMClientFactory>();
            var llmClientStratgey = llmFactory.CreateClient(options.ProviderType);

            return new LLMServiceManager(llmClientStratgey);
        });

        builder.AddClients();

        builder.Services.AddSingleton<CodeRAGService>();
        builder.Services.AddSingleton<CodeLoaderService>();
        builder.Services.AddSingleton<EmbeddingService>();
        builder.Services.AddSingleton<EmbeddingsStore>();
        builder.Services.AddSingleton<VectorDatabase>();
        builder.Services.AddSingleton<ModifyService>();

        builder.Services.AddSingleton<CodeAssistCommand>();
        builder.Services.AddSingleton<CodeInterpreterCommand>();
        builder.Services.AddSingleton<ChatAssistCommand>();
        builder.Services.AddSingleton<TreeStructureCommand>();
        builder.Services.AddSingleton<AIAssistCommand>();

        return builder;
    }
}
