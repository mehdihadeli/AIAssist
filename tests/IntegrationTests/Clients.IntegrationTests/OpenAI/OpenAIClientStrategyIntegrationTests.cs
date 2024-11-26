using Clients.Contracts;
using Clients.Dtos;
using Clients.Models;
using Clients.Options;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Clients.IntegrationTests.OpenAI;

[Collection(ApplicationCollection.Name)]
public class OpenAIClientStrategyIntegrationTests(ApplicationFixture applicationFixture) : IAsyncLifetime
{
    private IHost _app = default!;
    private ILLMClient _illmClient = default!;

    public Task InitializeAsync()
    {
        _app = applicationFixture.App;
        var llmOptions = _app.Services.GetRequiredService<IOptions<LLMOptions>>();
        llmOptions.Value.BaseAddress = ClientsConstants.Openai.BaseAddress;
        llmOptions.Value.ChatModel = ClientsConstants.Openai.ChatModels.GPT3_5Turbo;
        llmOptions.Value.EmbeddingsModel = ClientsConstants.Openai.EmbeddingsModels.TextEmbedding3Small;

        var clientFactory = _app.Services.GetRequiredService<ILLMClientFactory>();
        _illmClient = clientFactory.CreateClient(AIProvider.Openai);

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetCompletionAsync_ShouldReturnCompletion_WhenResponseIsSuccessful()
    {
        // Arrange
        var chatItems = new List<ChatCompletionRequestItem> { new(Role: RoleType.User, Prompt: "Hello") };

        // Act
        var result = await _illmClient.GetCompletionAsync(new ChatCompletionRequest(chatItems));

        // Assert
        result.Should().NotBeNull();
        result?.ChatResponse.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetEmbeddingAsync_ShouldReturnEmbedding_WhenResponseIsSuccessful()
    {
        // Arrange
        var input = "test input";

        // Act
        var result = await _illmClient.GetEmbeddingAsync(input);

        // Assert
        result.Should().NotBeNull();
        result?.Embeddings.Should().NotBeNull();
        result?.Embeddings.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCompletionStreamAsync_ShouldReturnMessages()
    {
        // Arrange
        var chatItems = new List<ChatCompletionRequestItem> { new(Role: RoleType.User, Prompt: "Hello, how are you?") };

        // Act
        var messages = await _illmClient.GetCompletionStreamAsync(new ChatCompletionRequest(chatItems)).ToListAsync();

        // Assert
        messages.Should().NotBeNull();
        messages.Should().HaveCountGreaterThan(0);
        messages[0]?.ChatResponse.Should().NotBeNullOrEmpty();
    }
}
