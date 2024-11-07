using Clients.Contracts;
using Clients.Dtos;
using Clients.Models;
using Clients.Options;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Clients.IntegrationTests.Anthropic;

[Collection(ApplicationCollection.Name)]
public class AnthropicClientStrategyIntegrationTests(ApplicationFixture applicationFixture) : IAsyncLifetime
{
    private IHost _app = default!;
    private ILLMClient _illmClient = default!;

    public Task InitializeAsync()
    {
        _app = applicationFixture.App;
        var llmOptions = _app.Services.GetRequiredService<IOptions<LLMOptions>>();
        llmOptions.Value.BaseAddress = ClientsConstants.Anthropic.BaseAddress;
        llmOptions.Value.ChatModel = ClientsConstants.Anthropic.ChatModels.Claude_3_5_Sonnet;
        llmOptions.Value.EmbeddingsModel = "";

        var clientFactory = _app.Services.GetRequiredService<ILLMClientFactory>();
        _illmClient = clientFactory.CreateClient(AIProvider.Anthropic);

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
        var chatItems = new List<ChatCompletionRequestItem> { new(Role: RoleType.User, Prompt: "Hello, Claude!") };

        // Act
        var result = await _illmClient.GetCompletionAsync(new ChatCompletionRequest(chatItems));

        // Assert
        result.Should().NotBeNull();
        result?.ChatResponse.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetCompletionStreamAsync_ShouldReturnStreamedMessages()
    {
        // Arrange
        var chatItems = new List<ChatCompletionRequestItem> { new(Role: RoleType.User, Prompt: "Say hello") };

        var cancellationToken = CancellationToken.None;

        // Act
        var messages = await _illmClient
            .GetCompletionStreamAsync(new ChatCompletionRequest(chatItems), cancellationToken)
            .ToListAsync(cancellationToken: cancellationToken);

        // Assert
        messages.Should().NotBeNullOrEmpty();
        messages.Should().HaveCountGreaterThan(0);
        messages.First()?.ChatResponse.Should().NotBeNullOrEmpty();
    }
}
