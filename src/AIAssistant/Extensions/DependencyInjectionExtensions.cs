using System.Net.Http.Headers;
using AIAssistant.Commands;
using AIAssistant.Contracts;
using AIAssistant.Data;
using AIAssistant.Models;
using AIAssistant.Models.Options;
using AIAssistant.Services;
using AIAssistant.Services.CodeAssistStrategies;
using BuildingBlocks.Extensions;
using BuildingBlocks.InMemoryVectorDatabase;
using BuildingBlocks.Serialization;
using Clients;
using Clients.Anthropic;
using Clients.Contracts;
using Clients.Models;
using Clients.Ollama;
using Clients.OpenAI;
using Clients.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Timeout;
using Polly.Wrap;
using Spectre.Console;

namespace AIAssistant.Extensions;

public static class DependencyInjectionExtensions
{
    public static HostApplicationBuilder AddDependencies(this HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton(AnsiConsole.Console);
        builder.Services.AddSingleton<IJsonSerializer, JsonObjectSerializer>();

        // will set json options for httpclient AsJson and FromJson
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonObjectSerializer.Options.PropertyNamingPolicy;
            options.SerializerOptions.WriteIndented = JsonObjectSerializer.Options.WriteIndented;
        });

        builder.Services.AddSingleton<AsyncPolicyWrap<HttpResponseMessage>>(sp =>
        {
            var policyOptions = sp.GetRequiredService<IOptions<PolicyOptions>>();

            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .RetryAsync(policyOptions.Value.RetryCount);

            // HttpClient itself will still enforce its own timeout, which is 100 seconds by default. To fix this issue, you need to set the HttpClient.Timeout property to match or exceed the timeout configured in Polly's policy.
            var timeoutPolicy = Policy.TimeoutAsync(policyOptions.Value.TimeoutSeconds, TimeoutStrategy.Pessimistic);

            // at any given time there will 3 parallel requests execution for specific service call and another 6 requests for other services can be in the queue. So that if the response from customer service is delayed or blocked then we don’t use too many resources
            var bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(3, 6);

            // https://github.com/App-vNext/Polly#handing-return-values-and-policytresult
            var circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(
                    policyOptions.Value.RetryCount + 1,
                    TimeSpan.FromSeconds(policyOptions.Value.BreakDuration)
                );

            var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, bulkheadPolicy);

            return combinedPolicy.WrapAsync(timeoutPolicy);
        });

        // Options
        builder.AddConfigurationOptions<CodeAssistOptions>(nameof(CodeAssistOptions));
        builder.AddConfigurationOptions<LogOptions>(nameof(LogOptions));
        builder.AddConfigurationOptions<LLMOptions>(nameof(LLMOptions));
        builder.AddConfigurationOptions<PolicyOptions>(nameof(PolicyOptions));

        // Clients
        builder.Services.AddKeyedSingleton<ILLMClientStratgey, OllamaClientStratgey>(AIProvider.Ollama);
        builder.Services.AddKeyedSingleton<ILLMClientStratgey, OpenAIClientStratgey>(AIProvider.OpenAI);
        builder.Services.AddKeyedSingleton<ILLMClientStratgey, AnthropicClientStratgey>(AIProvider.Anthropic);
        builder.Services.AddSingleton<ILLMClientManager, LLMClientManager>();

        builder.Services.AddSingleton<ILLMClientFactory, LLMClientFactory>(sp =>
        {
            var ollamaClient = sp.GetRequiredKeyedService<ILLMClientStratgey>(AIProvider.Ollama);
            var openAIClient = sp.GetRequiredKeyedService<ILLMClientStratgey>(AIProvider.OpenAI);
            var anthropicClient = sp.GetRequiredKeyedService<ILLMClientStratgey>(AIProvider.Anthropic);

            IDictionary<AIProvider, ILLMClientStratgey> clientStrategies = new Dictionary<
                AIProvider,
                ILLMClientStratgey
            >
            {
                { AIProvider.Ollama, ollamaClient },
                { AIProvider.OpenAI, openAIClient },
                { AIProvider.Anthropic, anthropicClient },
            };

            return new LLMClientFactory(clientStrategies);
        });

        // https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests
        builder.Services.AddHttpClient(
            "llm_client",
            (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<LLMOptions>>().Value;
                var policyOptions = sp.GetRequiredService<IOptions<PolicyOptions>>().Value;
                var modelStorage = sp.GetRequiredService<IModelsStorageService>();

                ArgumentException.ThrowIfNullOrEmpty(options.BaseAddress);
                ArgumentException.ThrowIfNullOrEmpty(options.ChatModel);

                client.BaseAddress = new Uri(options.BaseAddress);
                client.Timeout = TimeSpan.FromSeconds(policyOptions.TimeoutSeconds);

                var providerType = modelStorage.GetAIProviderFromModel(options.ChatModel, ModelType.ChatModel);

                switch (providerType)
                {
                    case AIProvider.Anthropic:
                        ArgumentException.ThrowIfNullOrEmpty(options.ApiKey);
                        client.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
                        // client.DefaultRequestHeaders.Add("anthropic-version", options.Version);
                        break;
                    case AIProvider.OpenAI:
                        ArgumentException.ThrowIfNullOrEmpty(options.ApiKey);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                            "Bearer",
                            options.ApiKey
                        );
                        break;
                    case AIProvider.Ollama:
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json")
                        );
                        break;
                }
            }
        );

        // Code Strategies
        builder.Services.TryAddKeyedSingleton<ICodeStrategy, EmbeddingCodeAssistStrategy>(
            CodeAssistStrategyType.Embedding
        );
        builder.Services.TryAddKeyedSingleton<ICodeStrategy, TreeSitterCodeAssistSummaryStrategy>(
            CodeAssistStrategyType.Summary
        );

        builder.Services.AddSingleton<ICodeAssistStrategyFactory>(sp =>
        {
            var embeddingCodeStrategy = sp.GetRequiredKeyedService<ICodeStrategy>(CodeAssistStrategyType.Embedding);
            var treeSitterSummaryCodeStrategy = sp.GetRequiredKeyedService<ICodeStrategy>(
                CodeAssistStrategyType.Summary
            );

            IDictionary<CodeAssistStrategyType, ICodeStrategy> codeStrategies = new Dictionary<
                CodeAssistStrategyType,
                ICodeStrategy
            >
            {
                { CodeAssistStrategyType.Embedding, embeddingCodeStrategy },
                { CodeAssistStrategyType.Summary, treeSitterSummaryCodeStrategy },
            };

            return new CodeAssistStrategyFactory(codeStrategies);
        });

        // Services
        builder.Services.AddSingleton<CodeFileMapService>();
        builder.Services.AddSingleton<CodeLoaderService>();
        builder.Services.AddSingleton<EmbeddingService>();
        builder.Services.AddSingleton<EmbeddingsStore>();
        builder.Services.AddSingleton<VectorDatabase>();
        builder.Services.AddSingleton<ModifyService>();
        builder.Services.AddSingleton<ICodeAssistantManager, CodeAssistantManager>();
        builder.Services.AddSingleton<IModelsStorageService, ModelsStorageService>(_ =>
        {
            var modelStorage = new ModelsStorageService();

            modelStorage.AddChatModel(new ChatModelStorage(Constants.Ollama.ChatModels.Llama3_1, AIProvider.Ollama));
            modelStorage.AddChatModel(
                new ChatModelStorage(Constants.Ollama.ChatModels.Deepseek_Coder_V2, AIProvider.Ollama)
            );

            modelStorage.AddEmbeddingModel(
                new EmbeddingModelStorage(Constants.Ollama.EmbeddingsModels.Mxbai_Embed_Large, AIProvider.Ollama, 0.6)
            );

            modelStorage.AddEmbeddingModel(
                new EmbeddingModelStorage(Constants.Ollama.EmbeddingsModels.Nomic_EmbedText, AIProvider.Ollama, 0.5)
            );

            modelStorage.AddChatModel(new ChatModelStorage(Constants.OpenAI.ChatModels.GPT3_5Turbo, AIProvider.OpenAI));

            modelStorage.AddChatModel(new ChatModelStorage(Constants.OpenAI.ChatModels.GPT4Mini, AIProvider.OpenAI));

            modelStorage.AddEmbeddingModel(
                new EmbeddingModelStorage(Constants.OpenAI.EmbeddingsModels.TextEmbedding3Large, AIProvider.OpenAI, 0.3)
            );

            modelStorage.AddEmbeddingModel(
                new EmbeddingModelStorage(Constants.OpenAI.EmbeddingsModels.TextEmbedding3Small, AIProvider.OpenAI, 0.2)
            );

            modelStorage.AddEmbeddingModel(
                new EmbeddingModelStorage(
                    Constants.OpenAI.EmbeddingsModels.TextEmbeddingAda_002,
                    AIProvider.OpenAI,
                    0.3
                )
            );

            modelStorage.AddChatModel(
                new ChatModelStorage(Constants.Anthropic.ChatModels.Claude_3_5_Sonnet, AIProvider.Anthropic)
            );

            return modelStorage;
        });

        // Commands
        builder.Services.AddSingleton<CodeAssistCommand>();
        builder.Services.AddSingleton<CodeInterpreterCommand>();
        builder.Services.AddSingleton<ChatAssistCommand>();
        builder.Services.AddSingleton<TreeStructureCommand>();
        builder.Services.AddSingleton<AIAssistCommand>();

        return builder;
    }
}
