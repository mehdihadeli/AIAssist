using System.Net.Http.Headers;
using Clients.Anthropic;
using Clients.Ollama;
using Clients.OpenAI;
using Clients.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AIAssistant.Extensions.HostApplicationBuilderExtensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddClients(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<OpenAiClientStratgey>(
            (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<LLMOptions>>().Value;
                var policyOptions = sp.GetRequiredService<IOptions<PolicyOptions>>().Value;

                ArgumentException.ThrowIfNullOrEmpty(options.BaseAddress);
                ArgumentException.ThrowIfNullOrEmpty(options.ApiKey);

                client.BaseAddress = new Uri(options.BaseAddress);
                client.Timeout = TimeSpan.FromSeconds(policyOptions.TimeoutSeconds);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
            }
        );

        builder.Services.AddHttpClient<OllamaClientStratgey>(
            (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<LLMOptions>>().Value;
                var policyOptions = sp.GetRequiredService<IOptions<PolicyOptions>>().Value;

                ArgumentException.ThrowIfNullOrEmpty(options.BaseAddress);

                client.BaseAddress = new Uri(options.BaseAddress);
                client.Timeout = TimeSpan.FromSeconds(policyOptions.TimeoutSeconds);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
        );

        builder.Services.AddHttpClient<AnthropicClientStratgey>(
            (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<LLMOptions>>().Value;
                var policyOptions = sp.GetRequiredService<IOptions<PolicyOptions>>().Value;

                ArgumentException.ThrowIfNullOrEmpty(options.BaseAddress);
                ArgumentException.ThrowIfNullOrEmpty(options.ApiKey);

                client.BaseAddress = new Uri(options.BaseAddress);
                client.Timeout = TimeSpan.FromSeconds(policyOptions.TimeoutSeconds);
                client.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
                // client.DefaultRequestHeaders.Add("anthropic-version", options.Version);
            }
        );

        return builder;
    }
}
