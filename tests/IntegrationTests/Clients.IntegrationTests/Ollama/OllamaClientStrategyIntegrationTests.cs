using Clients.Chat.Models;
using Clients.Contracts;
using Clients.Models;
using Clients.Options;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Clients.IntegrationTests.Ollama;

[Collection(ApplicationCollection.Name)]
public class OllamaClientStrategyIntegrationTests(ApplicationFixture applicationFixture) : IAsyncLifetime
{
    private IHost _app = default!;
    private ILLMClientStratgey _llmClientStratgey = default!;

    public Task InitializeAsync()
    {
        _app = applicationFixture.App;
        var llmOptions = _app.Services.GetRequiredService<IOptions<LLMOptions>>();
        llmOptions.Value.BaseAddress = ClientsConstants.Ollama.BaseAddress;
        llmOptions.Value.ChatModel = ClientsConstants.Ollama.ChatModels.Llama3_1;
        llmOptions.Value.EmbeddingsModel = ClientsConstants.Ollama.EmbeddingsModels.Mxbai_Embed_Large;

        var clientFactory = _app.Services.GetRequiredService<ILLMClientFactory>();
        _llmClientStratgey = clientFactory.CreateClient(AIProvider.Ollama);

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetCompletionAsync_ShouldReturnCompletion_WhenResponseIsSuccessful()
    {
        // Act
        var chatItems = new List<ChatItem> { new(Role: RoleType.User, Prompt: "Hello") };
        var result = await _llmClientStratgey.GetCompletionAsync(chatItems);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetEmbeddingAsync_ShouldReturnEmbedding_WhenResponseIsSuccessful()
    {
        // Arrange
        var input = "test input";

        // Act
        var result = await _llmClientStratgey.GetEmbeddingAsync(input);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCompletionStreamAsync_ShouldReturnMessages()
    {
        // Arrange
        var chatItems = new List<ChatItem> { new(Role: RoleType.User, Prompt: "Hello, how are you?") };

        // Act
        var messages = await _llmClientStratgey.GetCompletionStreamAsync(chatItems).ToListAsync();

        // Assert
        messages.Should().NotBeEmpty();
        messages[0].Should().NotBeNullOrEmpty();
    }
}
