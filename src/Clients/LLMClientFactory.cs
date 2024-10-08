using Clients.Anthropic;
using Clients.Contracts;
using Clients.Models;
using Clients.Ollama;
using Clients.OpenAI;
using Microsoft.Extensions.DependencyInjection;

namespace Clients;

public class LLMClientFactory(IServiceProvider serviceProvider)
{
    public ILLMClientStratgey CreateClient(AIProvider aiProvider)
    {
        return aiProvider switch
        {
            AIProvider.OpenAI => serviceProvider.GetRequiredService<OpenAiClientStratgey>(),
            AIProvider.Ollama => serviceProvider.GetRequiredService<OllamaClientStratgey>(),
            AIProvider.Anthropic => serviceProvider.GetRequiredService<AnthropicClientStratgey>(),
            _ => throw new ArgumentException("Invalid client type"),
        };
    }
}
