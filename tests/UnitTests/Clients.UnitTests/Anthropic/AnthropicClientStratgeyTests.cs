using System.Net;
using System.Net.Http.Json;
using System.Text;
using BuildingBlocks.Serialization;
using Clients.Chat.Models;
using Clients.Dtos;
using Clients.Models;
using Clients.Models.Anthropic;
using Clients.Options;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Polly;
using RichardSzalay.MockHttp;

namespace Clients.UnitTests.Anthropic;

public class AnthropicClientStrategyTests : IAsyncLifetime
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly AnthropicClient _clientStrategy;
    private readonly Uri _clientBaseUri;

    public AnthropicClientStrategyTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();

        IOptions<LLMOptions> llmOptions = Substitute.For<IOptions<LLMOptions>>();
        IOptions<PolicyOptions> policyOptions = Substitute.For<IOptions<PolicyOptions>>();

        var llmOptionsValue = new LLMOptions
        {
            ChatModel = "claude-v1",
            EmbeddingsModel = "voyage-embed",
            BaseAddress = "http://localhost",
        };

        var policyOptionsValue = new PolicyOptions
        {
            RetryCount = 3,
            TimeoutSeconds = 30,
            BreakDuration = 60,
        };

        llmOptions.Value.Returns(llmOptionsValue);
        policyOptions.Value.Returns(policyOptionsValue);
        _clientBaseUri = new Uri(llmOptionsValue.BaseAddress);

        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .RetryAsync(policyOptionsValue.RetryCount);

        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(policyOptionsValue.TimeoutSeconds);

        var bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(3, 6);

        var circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                policyOptionsValue.RetryCount + 1,
                TimeSpan.FromSeconds(policyOptionsValue.BreakDuration)
            );

        var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, bulkheadPolicy);
        combinedPolicy = combinedPolicy.WrapAsync(timeoutPolicy);

        // Create a MockHttpMessageHandler and setup the HttpClient
        var client = _mockHttp.ToHttpClient();
        client.BaseAddress = _clientBaseUri;

        // Configure the IHttpClientFactory to return the mocked HttpClient
        httpClientFactory.CreateClient("llm_client").Returns(client);

        _clientStrategy = new AnthropicClient(httpClientFactory, llmOptions, combinedPolicy);
    }

    [Fact]
    public async Task GetCompletionAsync_ShouldReturnCompletion_WhenResponseIsSuccessful()
    {
        // Arrange
        var chatItems = new List<ChatCompletionRequestItem> { new(Role: RoleType.User, Prompt: "Hello, Claude!") };

        var responseContent = new AnthropicCompletionResponse
        {
            Content = new List<MessageContent>
            {
                new() { Text = "Hello, how can I assist you today?", Type = "text" },
            },
        };

        _mockHttp
            .When(HttpMethod.Post, $"{_clientBaseUri}v1/complete")
            .Respond(JsonContent.Create(responseContent, options: JsonObjectSerializer.SnakeCaseOptions));

        // Act
        var result = await _clientStrategy.GetCompletionAsync(chatItems);

        // Assert
        result.Should().Be("Hello, how can I assist you today?");
    }

    [Fact]
    public async Task GetCompletionAsync_ShouldThrowException_WhenResponseIsUnsuccessful()
    {
        // Arrange
        var chatItems = new List<ChatItem> { new(Role: RoleType.User, Prompt: "Hello, Claude!") };

        _mockHttp.When(HttpMethod.Post, $"{_clientBaseUri}v1/complete").Respond(HttpStatusCode.BadRequest);

        // Act
        Func<Task> act = async () => await _clientStrategy.GetCompletionAsync(chatItems);

        // Assert
        var exception = await act.Should().ThrowAsync<HttpRequestException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetEmbeddingAsync_ShouldThrow_NotImplementedException()
    {
        // Arrange
        var input = "test input";

        _mockHttp.When(HttpMethod.Post, $"{_clientBaseUri}v1/embeddings").Respond(HttpStatusCode.BadRequest);

        // Act
        Func<Task> act = async () => await _clientStrategy.GetEmbeddingAsync(input);

        // Assert
        await act.Should().ThrowAsync<NotImplementedException>();
    }

    [Fact]
    public async Task GetCompletionStreamAsync_ShouldReturnStreamedMessages()
    {
        // Arrange
        var chatItems = new List<ChatItem> { new(Role: RoleType.User, Prompt: "Say hello") };

        var streamingResponse = new StringBuilder();
        streamingResponse.AppendLine("data: {\"content\":[{\"type\":\"text_delta\",\"text\":\"Hello\"}]}");
        streamingResponse.AppendLine("data: {\"content\":[{\"type\":\"text_delta\",\"text\":\"World\"}]}");
        streamingResponse.AppendLine("data: ");

        _mockHttp
            .When(HttpMethod.Post, $"{_clientBaseUri}v1/complete")
            .Respond("text/event-stream", streamingResponse.ToString());

        var cancellationToken = CancellationToken.None;

        // Act
        var messages = await _clientStrategy
            .GetCompletionStreamAsync(chatItems, cancellationToken)
            .ToListAsync(cancellationToken: cancellationToken);

        // Assert
        messages.Should().HaveCount(2);
        messages[0].Should().Be("Hello");
        messages[1].Should().Be("World");
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _mockHttp.Dispose();
        return Task.CompletedTask;
    }
}
