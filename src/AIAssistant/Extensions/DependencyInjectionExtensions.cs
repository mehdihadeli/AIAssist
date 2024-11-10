using System.Net.Http.Headers;
using AIAssistant.Commands;
using AIAssistant.Commands.InternalCommands;
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
using BuildingBlocks.InMemoryVectorDatabase.Contracts;
using BuildingBlocks.LLM;
using BuildingBlocks.LLM.Tokenizers;
using BuildingBlocks.Serialization;
using BuildingBlocks.SpectreConsole;
using BuildingBlocks.SpectreConsole.Contracts;
using Clients;
using Clients.Contracts;
using Clients.Models;
using Clients.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Timeout;
using Polly.Wrap;
using Spectre.Console;
using TreeSitter.Bindings.Contracts;
using TreeSitter.Bindings.Services;
using TreeSitter.Bindings.Utilities;

namespace AIAssistant.Extensions;

public static class DependencyInjectionExtensions
{
    public static HostApplicationBuilder AddDependencies(this HostApplicationBuilder builder)
    {
        AddSpectreConsoleDependencies(builder);

        AddCodeTreeMappingDependencies(builder);

        AddResiliencyDependencies(builder);

        AddJsonSerializerDependencies(builder);

        AddInMemoryCache(builder);

        AddOptionsDependencies(builder);

        AddClientDependencies(builder);

        AddCodeAssistDependencies(builder);

        AddEmbeddingDependencies(builder);

        AddCodeDiffDependency(builder);

        AddPromptDependencies(builder);

        AddCacheModelsDependencies(builder);

        AddCommandsDependencies(builder);

        AddChatDependencies(builder);

        AddTokenizersDependencies(builder);

        return builder;
    }

    private static void AddInMemoryCache(HostApplicationBuilder builder)
    {
        builder.Services.AddMemoryCache();
    }

    private static void AddTokenizersDependencies(HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ITokenizer, GptTokenizer>();
    }

    private static void AddChatDependencies(HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IChatSessionManager, ChatSessionManager>();
    }

    private static void AddSpectreConsoleDependencies(HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton(AnsiConsole.Console);

        builder.Services.AddSingleton<ISpectreUtilities>(sp =>
        {
            var appOptions = sp.GetRequiredService<AppOptions>();
            var console = sp.GetRequiredService<IAnsiConsole>();
            return new SpectreUtilities(ThemeLoader.LoadTheme(appOptions.ThemeName)!, console);
        });
    }

    private static void AddCodeTreeMappingDependencies(HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ICodeFileTreeGeneratorService, CodeFilesTreeGeneratorService>();
        builder.Services.AddSingleton<ITreeSitterParser, TreeSitterParser>();
        builder.Services.AddSingleton<ITreeSitterCodeCaptureService, TreeSitterCodeCaptureService>();
        builder.Services.AddSingleton<ITreeStructureGeneratorService, TreeStructureGeneratorService>();
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
            options.SerializerOptions.PropertyNamingPolicy = JsonObjectSerializer.SnakeCaseOptions.PropertyNamingPolicy;

            options.SerializerOptions.WriteIndented = JsonObjectSerializer.SnakeCaseOptions.WriteIndented;
        });

        builder.Services.AddSingleton<IJsonSerializer, JsonObjectSerializer>();
    }

    private static void AddCacheModelsDependencies(HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ICacheModels, CacheModels>();
    }

    private static void AddOptionsDependencies(HostApplicationBuilder builder)
    {
        builder.AddConfigurationOptions<AppOptions>(nameof(AppOptions));
        builder.AddConfigurationOptions<ModelsOptions>(nameof(ModelsOptions));
        builder.AddConfigurationOptions<ModelsInformationOptions>(nameof(ModelsInformationOptions));
        builder.AddConfigurationOptions<LLMOptions>(nameof(LLMOptions));
        builder.AddConfigurationOptions<PolicyOptions>(nameof(PolicyOptions));
    }

    private static void AddEmbeddingDependencies(HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IEmbeddingService, EmbeddingService>();
        builder.Services.AddSingleton<ICodeEmbeddingsRepository, CodeEmbeddingsRepository>();
        builder.Services.AddSingleton<IVectorContext, VectorContext>();
    }

    private static void AddCodeAssistDependencies(HostApplicationBuilder builder)
    {
        builder.Services.TryAddKeyedScoped<ICodeAssist, EmbeddingCodeAssist>(CodeAssistType.Embedding);

        builder.Services.TryAddKeyedScoped<ICodeAssist, TreeSitterCodeAssistSummary>(CodeAssistType.Summary);

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

        builder.Services.AddScoped<ICodeAssistantManager, CodeAssistantManager>(sp =>
        {
            var factory = sp.GetRequiredService<ICodeAssistFactory>();
            var llmOptions = sp.GetRequiredService<IOptions<LLMOptions>>();
            var codeDiffManager = sp.GetRequiredService<ICodeDiffManager>();
            var cacheModels = sp.GetRequiredService<ICacheModels>();

            var chatModel = cacheModels.GetModel(llmOptions.Value.ChatModel);

            ICodeAssist codeAssist = factory.Create(chatModel.ModelOption.CodeAssistType);

            return new CodeAssistantManager(codeAssist, codeDiffManager);
        });
    }

    private static void AddPromptDependencies(HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IPromptCache, PromptCache>(_ =>
        {
            var promptStorage = new PromptCache();

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
        builder.Services.AddKeyedSingleton<ILLMClient, OpenAiClient>(AIProvider.Openai);
        //builder.Services.AddKeyedSingleton<ILLMClient, AnthropicClient>(AIProvider.Anthropic);
        builder.Services.AddSingleton<ILLMClientManager, LLMClientManager>();

        builder.Services.AddSingleton<ILLMClientFactory, LLMClientFactory>(sp =>
        {
            var ollamaClient = sp.GetRequiredKeyedService<ILLMClient>(AIProvider.Ollama);
            var openAIClient = sp.GetRequiredKeyedService<ILLMClient>(AIProvider.Openai);
            //var anthropicClient = sp.GetRequiredKeyedService<ILLMClient>(AIProvider.Anthropic);

            IDictionary<AIProvider, ILLMClient> clientStrategies = new Dictionary<AIProvider, ILLMClient>
            {
                { AIProvider.Ollama, ollamaClient },
                { AIProvider.Openai, openAIClient },
                //{ AIProvider.Anthropic, anthropicClient },
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

                var cacheModels = sp.GetRequiredService<ICacheModels>();

                ArgumentException.ThrowIfNullOrEmpty(options.BaseAddress);
                ArgumentException.ThrowIfNullOrEmpty(options.ChatModel);

                client.BaseAddress = new Uri(options.BaseAddress);
                client.Timeout = TimeSpan.FromSeconds(policyOptions.TimeoutSeconds);

                var model = cacheModels.GetModel(options.ChatModel);

                switch (model.ModelInformation.AIProvider)
                {
                    // case AIProvider.Anthropic:
                    //     ArgumentException.ThrowIfNullOrEmpty(options.ApiKey);
                    //     client.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
                    //     // client.DefaultRequestHeaders.Add("anthropic-version", options.Version);
                    //     break;
                    case AIProvider.Openai:
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
        builder.Services.AddTransient<IInternalCommandProcessor, InternalCommandProcessor>();
        builder.Services.AddSingleton<ICodeAssistInternalCommands>(sp =>
        {
            IList<IInternalConsoleCommand> internalCommands =
            [
                sp.GetRequiredService<AddFileCommand>(),
                sp.GetRequiredService<RunCommand>(),
                sp.GetRequiredService<ClearCommand>(),
                sp.GetRequiredService<ClearHistoryCommand>(),
                sp.GetRequiredService<QuitCommand>(),
                sp.GetRequiredService<FoldersTreeListCommand>(),
                sp.GetRequiredService<TokenCommand>(),
                sp.GetRequiredService<SummarizeCommand>(),
            ];

            var help =
                @"- `:clear` / `:c` / Ctrl+F - Clear the conversation.
- `:quit` / `:q` / Ctrl+C - Quit the program.
- `:help` / `:h` / `:?` - Show this help message.";
            var spectreUtils = sp.GetRequiredService<ISpectreUtilities>();
            internalCommands.Add(new HelpCommand(help, spectreUtils));

            var codeAssistInternalCommands = new CodeAssistInternalCommands();
            codeAssistInternalCommands.AddRange(internalCommands!);

            return codeAssistInternalCommands;
        });

        // internal commands
        builder.Services.AddTransient<AddFileCommand>();
        builder.Services.AddTransient<RunCommand>();
        builder.Services.AddTransient<ClearCommand>();
        builder.Services.AddTransient<ClearHistoryCommand>();
        builder.Services.AddTransient<QuitCommand>();
        builder.Services.AddTransient<FoldersTreeListCommand>();
        builder.Services.AddTransient<HelpCommand>();
        builder.Services.AddTransient<TokenCommand>();
        builder.Services.AddTransient<SummarizeCommand>();

        // commands
        builder.Services.AddTransient<CodeAssistCommand>();
        builder.Services.AddTransient<CodeExplanationCommand>();
        builder.Services.AddTransient<ChatAssistCommand>();
        builder.Services.AddTransient<TreeStructureCommand>();
        builder.Services.AddTransient<AIAssistCommand>();
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
            var options = sp.GetRequiredService<IOptions<LLMOptions>>();
            var factory = sp.GetRequiredService<ICodeDiffParserFactory>();
            var cacheModels = sp.GetRequiredService<ICacheModels>();
            var chatModel = cacheModels.GetModel(options.Value.ChatModel);

            var codeDiffParser = factory.Create(chatModel.ModelOption.CodeDiffType);

            var codeDiffUpdater = sp.GetRequiredService<ICodeDiffUpdater>();

            return new CodeDiffManager(codeDiffParser, codeDiffUpdater);
        });
    }
}
