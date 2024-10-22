using Clients.Chat.Models;
using Clients.Contracts;
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
    private ILLMClientStratgey _llmClientStratgey = default!;

    public Task InitializeAsync()
    {
        _app = applicationFixture.App;
        var llmOptions = _app.Services.GetRequiredService<IOptions<LLMOptions>>();
        llmOptions.Value.BaseAddress = Constants.Anthropic.BaseAddress;
        llmOptions.Value.ChatModel = Constants.Anthropic.ChatModels.Claude_3_5_Sonnet;
        llmOptions.Value.EmbeddingsModel = "";

        var clientFactory = _app.Services.GetRequiredService<ILLMClientFactory>();
        _llmClientStratgey = clientFactory.CreateClient(AIProvider.Anthropic);

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
        var chatItems = new List<ChatItem> { new(Role: RoleType.User, Prompt: "Hello, Claude!") };

        // Act
        var result = await _llmClientStratgey.GetCompletionAsync(chatItems);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetCompletionStreamAsync_ShouldReturnStreamedMessages()
    {
        // Arrange
        var chatItems = new List<ChatItem> { new(Role: RoleType.User, Prompt: "Say hello") };

        var cancellationToken = CancellationToken.None;

        // Act
        var messages = await _llmClientStratgey
            .GetCompletionStreamAsync(chatItems, cancellationToken)
            .ToListAsync(cancellationToken: cancellationToken);

        // Assert
        messages.Should().NotBeNullOrEmpty();
        messages[0].Should().NotBeNullOrEmpty();
    }
}
