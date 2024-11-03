using System.Net.Http.Headers;
using AIAssistant.Commands;
using AIAssistant.Contracts;
using AIAssistant.Contracts.CodeAssist;
using AIAssistant.Contracts.Diff;
using AIAssistant.Data;
using AIAssistant.Diff;
using AIAssistant.Models;
using AIAssistant.Models.Options;
using AIAssistant.Prompts;
using AIAssistant.Services;
using AIAssistant.Services.CodeAssistStrategies;
using BuildingBlocks.Extensions;
using BuildingBlocks.InMemoryVectorDatabase;
using BuildingBlocks.Serialization;
using BuildingBlocks.SpectreConsole;
using BuildingBlocks.SpectreConsole.Contracts;
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
        AddSpectreConsoleDependencies(builder);

        AddCodeLoaderDependencies(builder);

        AddResiliencyDependencies(builder);

        AddJsonSerializerDependencies(builder);

        AddOptionsDependencies(builder);

        AddClientDependencies(builder);

        AddCodeAssistDependencies(builder);

        AddEmbeddingDependencies(builder);

        AddCodeDiffDependency(builder);

        AddPromptDependencies(builder);

        AddModelStorageDependencies(builder);

        AddCommandsDependencies(builder);

        return builder;
    }

    private static void AddSpectreConsoleDependencies(HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IAnsiConsole>(AnsiConsole.Console);
        builder.Services.AddSingleton<ISpectreConsoleUtilities>(sp =>
        {
            var appOptions = sp.GetRequiredService<IOptions<AppOptions>>();
            var console = sp.GetRequiredService<IAnsiConsole>();
            return new SpectreConsoleUtilities(ThemeLoader.LoadTheme(appOptions.Value.ThemeName)!, console);
        });
    }

    private static void AddCodeLoaderDependencies(HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ICodeFileMapService, CodeFileMapService>();
        builder.Services.AddSingleton<CodeLoaderService>();
    }

    private static void AddResiliencyDependencies(HostApplicationBuilder builder)
    {
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
    }

    private static void AddJsonSerializerDependencies(HostApplicationBuilder builder)
    {
        // will set json options for httpclient AsJson and FromJson
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonObjectSerializer.Options.PropertyNamingPolicy;
            options.SerializerOptions.WriteIndented = JsonObjectSerializer.Options.WriteIndented;
        });

        builder.Services.AddSingleton<IJsonSerializer, JsonObjectSerializer>();
    }

    private static void AddModelStorageDependencies(HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IModelsStorageService, ModelsStorageService>(_ =>
        {
            var modelStorage = new ModelsStorageService();

            modelStorage.AddChatModel(
                new ChatModelStorage(ClientsConstants.Ollama.ChatModels.Llama3_1, AIProvider.Ollama)
            );
            modelStorage.AddChatModel(
                new ChatModelStorage(ClientsConstants.Ollama.ChatModels.Deepseek_Coder_V2, AIProvider.Ollama)
            );

            modelStorage.AddEmbeddingModel(
                new EmbeddingModelStorage(
                    ClientsConstants.Ollama.EmbeddingsModels.Mxbai_Embed_Large,
                    AIProvider.Ollama,
                    0.5
                )
            );

            modelStorage.AddEmbeddingModel(
                new EmbeddingModelStorage(
                    ClientsConstants.Ollama.EmbeddingsModels.Nomic_EmbedText,
                    AIProvider.Ollama,
                    0.4
                )
            );

            modelStorage.AddChatModel(
                new ChatModelStorage(ClientsConstants.OpenAI.ChatModels.GPT3_5Turbo, AIProvider.OpenAI)
            );

            modelStorage.AddChatModel(
                new ChatModelStorage(ClientsConstants.OpenAI.ChatModels.GPT4Mini, AIProvider.OpenAI)
            );

            modelStorage.AddEmbeddingModel(
                new EmbeddingModelStorage(
                    ClientsConstants.OpenAI.EmbeddingsModels.TextEmbedding3Large,
                    AIProvider.OpenAI,
                    0.3
                )
            );

            modelStorage.AddEmbeddingModel(
                new EmbeddingModelStorage(
                    ClientsConstants.OpenAI.EmbeddingsModels.TextEmbedding3Small,
                    AIProvider.OpenAI,
                    0.2
                )
            );

            modelStorage.AddEmbeddingModel(
                new EmbeddingModelStorage(
                    ClientsConstants.OpenAI.EmbeddingsModels.TextEmbeddingAda_002,
                    AIProvider.OpenAI,
                    0.3
                )
            );

            modelStorage.AddChatModel(
                new ChatModelStorage(ClientsConstants.Anthropic.ChatModels.Claude_3_5_Sonnet, AIProvider.Anthropic)
            );

            return modelStorage;
        });
    }

    private static void AddOptionsDependencies(HostApplicationBuilder builder)
    {
        builder.AddConfigurationOptions<CodeAssistOptions>(nameof(CodeAssistOptions));
        builder.AddConfigurationOptions<AppOptions>(nameof(AppOptions));
        builder.AddConfigurationOptions<PolicyOptions>(nameof(PolicyOptions));
    }

    private static void AddEmbeddingDependencies(HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<EmbeddingService>();
        builder.Services.AddSingleton<EmbeddingsStore>();
        builder.Services.AddSingleton<VectorDatabase>();
    }

    private static void AddCodeAssistDependencies(HostApplicationBuilder builder)
    {
        builder.Services.TryAddKeyedSingleton<ICodeAssist, EmbeddingCodeAssist>(CodeAssistType.Embedding);
        builder.Services.TryAddKeyedSingleton<ICodeAssist, TreeSitterCodeAssistSummary>(CodeAssistType.Summary);

        builder.Services.AddSingleton<ICodeAssistFactory>(sp =>
        {
            var embeddingCodeStrategy = sp.GetRequiredKeyedService<ICodeAssist>(CodeAssistType.Embedding);
            var treeSitterSummaryCodeStrategy = sp.GetRequiredKeyedService<ICodeAssist>(CodeAssistType.Summary);

            IDictionary<CodeAssistType, ICodeAssist> codeStrategies = new Dictionary<CodeAssistType, ICodeAssist>
            {
                { CodeAssistType.Embedding, embeddingCodeStrategy },
                { CodeAssistType.Summary, treeSitterSummaryCodeStrategy },
            };

            return new CodeAssistFactory(codeStrategies);
        });

        builder.Services.AddSingleton<ICodeAssistantManager, CodeAssistantManager>(sp =>
        {
            var factory = sp.GetRequiredService<ICodeAssistFactory>();
            var options = sp.GetRequiredService<IOptions<CodeAssistOptions>>();
            var codeDiffManager = sp.GetRequiredService<ICodeDiffManager>();

            ICodeAssist codeAssist = factory.Create(options.Value.CodeAssistType);

            return new CodeAssistantManager(codeAssist, codeDiffManager);
        });
    }

    private static void AddPromptDependencies(HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IPromptStorage, PromptStorage>(_ =>
        {
            var promptStorage = new PromptStorage();

            promptStorage.AddPrompt(
                AIAssistantConstants.Prompts.CodeAssistantUnifiedDiffTemplate,
                CommandType.Code,
                CodeDiffType.UnifiedDiff
            );

            promptStorage.AddPrompt(
                AIAssistantConstants.Prompts.CodeAssistantCodeBlockdDiffTemplate,
                CommandType.Code,
                CodeDiffType.CodeBlockDiff
            );

            promptStorage.AddPrompt(
                AIAssistantConstants.Prompts.CodeAssistantMergeConflictDiffTemplate,
                CommandType.Code,
                CodeDiffType.MergeConflictDiff
            );

            return promptStorage;
        });
    }

    private static void AddClientDependencies(HostApplicationBuilder builder)
    {
        builder.AddConfigurationOptions<LLMOptions>(nameof(LLMOptions));

        builder.Services.AddKeyedSingleton<ILLMClient, OllamaClient>(AIProvider.Ollama);
        builder.Services.AddKeyedSingleton<ILLMClient, OpenAiClient>(AIProvider.OpenAI);
        builder.Services.AddKeyedSingleton<ILLMClient, AnthropicClient>(AIProvider.Anthropic);
        builder.Services.AddSingleton<ILLMClientManager, LLMClientManager>();

        builder.Services.AddSingleton<ILLMClientFactory, LLMClientFactory>(sp =>
        {
            var ollamaClient = sp.GetRequiredKeyedService<ILLMClient>(AIProvider.Ollama);
            var openAIClient = sp.GetRequiredKeyedService<ILLMClient>(AIProvider.OpenAI);
            var anthropicClient = sp.GetRequiredKeyedService<ILLMClient>(AIProvider.Anthropic);

            IDictionary<AIProvider, ILLMClient> clientStrategies = new Dictionary<AIProvider, ILLMClient>
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
    }

    private static void AddCommandsDependencies(HostApplicationBuilder builder)
    {
        // Commands
        builder.Services.AddSingleton<CodeAssistCommand>();
        builder.Services.AddSingleton<CodeExplanationCommand>();
        builder.Services.AddSingleton<ChatAssistCommand>();
        builder.Services.AddSingleton<TreeStructureCommand>();
        builder.Services.AddSingleton<AIAssistCommand>();
    }

    private static void AddCodeDiffDependency(HostApplicationBuilder builder)
    {
        builder.Services.AddKeyedSingleton<ICodeDiffParser, CodeBlockCodeDiffParser>(CodeDiffType.CodeBlockDiff);
        builder.Services.AddKeyedSingleton<ICodeDiffParser, UnifiedCodeDiffParser>(CodeDiffType.UnifiedDiff);
        builder.Services.AddKeyedSingleton<ICodeDiffParser, MergeConflictCodeDiffParser>(
            CodeDiffType.MergeConflictDiff
        );
        builder.Services.AddSingleton<ICodeDiffUpdater, CodeDiffUpdater>();

        builder.Services.AddSingleton<ICodeDiffParserFactory, CodeDiffParserFactory>(sp =>
        {
            var codeBlockDiffParser = sp.GetRequiredKeyedService<ICodeDiffParser>(CodeDiffType.CodeBlockDiff);
            var unifiedDiffParser = sp.GetRequiredKeyedService<ICodeDiffParser>(CodeDiffType.UnifiedDiff);
            var mergeConflictDiffParser = sp.GetRequiredKeyedService<ICodeDiffParser>(CodeDiffType.MergeConflictDiff);

            IDictionary<CodeDiffType, ICodeDiffParser> codeDiffStrategies = new Dictionary<
                CodeDiffType,
                ICodeDiffParser
            >
            {
                { CodeDiffType.CodeBlockDiff, codeBlockDiffParser },
                { CodeDiffType.UnifiedDiff, unifiedDiffParser },
                { CodeDiffType.MergeConflictDiff, mergeConflictDiffParser },
            };

            return new CodeDiffParserFactory(codeDiffStrategies);
        });

        builder.Services.AddSingleton<ICodeDiffManager, CodeDiffManager>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CodeAssistOptions>>();
            var factory = sp.GetRequiredService<ICodeDiffParserFactory>();
            var codeDiffParser = factory.Create(options.Value.CodeDiffType);

            var codeDiffUpdater = sp.GetRequiredService<ICodeDiffUpdater>();

            return new CodeDiffManager(codeDiffParser, codeDiffUpdater);
        });
    }
}
