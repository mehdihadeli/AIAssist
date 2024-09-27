using System.Net.Http.Headers;
using AIRefactorAssistant.Options;
using Clients;
using Clients.Anthropic;
using Clients.Ollama;
using Clients.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AIRefactorAssistant.Extensions.HostApplicationBuilderExtensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddClients(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<ILanguageModelService, OpenAiService>(
            "OpenAiClient",
            (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<OpenAIOptions>>().Value;

                client.BaseAddress = new Uri(options.BaseAddress);
                client.Timeout = TimeSpan.FromMinutes(5);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
            }
        );

        builder.Services.AddHttpClient<ILanguageModelService, AnthropicService>(
            "AnthropicClient",
            (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<AnthropicOptions>>().Value;

                client.BaseAddress = new Uri(options.BaseAddress);
                client.Timeout = TimeSpan.FromMinutes(5);
                client.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
                // client.DefaultRequestHeaders.Add("anthropic-version", options.Version);
            }
        );

        builder.Services.AddHttpClient<ILanguageModelService, OllamaService>(
            "OllamaClient",
            (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;

                client.Timeout = TimeSpan.FromMinutes(5);
                client.BaseAddress = new Uri(options.BaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
        );

        return builder;
    }
}
