using System.Net;
using System.Net.Http.Json;
using System.Text;
using BuildingBlocks.Serialization;
using Clients.Chat.Models;
using Clients.Models;
using Clients.Models.Ollama.Embeddings;
using Clients.Models.OpenAI.Completion;
using Clients.Options;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Polly;
using RichardSzalay.MockHttp;

namespace Clients.UnitTests.OpenAI;

public class OpenAIClientStrategyTests : IAsyncLifetime
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly Uri _clientBaseUri;
    private readonly OpenAiClient _clientStrategy;

    public OpenAIClientStrategyTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();

        IOptions<LLMOptions> llmOptions = Substitute.For<IOptions<LLMOptions>>();
        IOptions<PolicyOptions> policyOptions = Substitute.For<IOptions<PolicyOptions>>();

        var llmOptionsValue = new LLMOptions
        {
            ChatModel = "gpt-3.5",
            MaxTokenSize = 256,
            EmbeddingsModel = "gpt-3.5-embed",
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

        // Create the HttpClient and setup IHttpClientFactory
        var client = _mockHttp.ToHttpClient();
        client.BaseAddress = _clientBaseUri;

        // Configure the IHttpClientFactory to return the mocked HttpClient
        httpClientFactory.CreateClient("llm_client").Returns(client);

        // Initialize the OpenAIClientStrategy with IHttpClientFactory
        _clientStrategy = new OpenAiClient(httpClientFactory, llmOptions, combinedPolicy);
    }

    [Fact]
    public async Task GetCompletionAsync_ShouldReturnCompletion_WhenResponseIsSuccessful()
    {
        // Arrange
        var chatItems = new List<ChatItem> { new(Role: RoleType.User, Prompt: "Hello") };

        var responseContent = new OpenAIChatResponse
        {
            Choices = new List<OpenAIChoice>
            {
                new()
                {
                    Message = new OpenAIMessage { Content = "Hello, how can I assist you?", Role = RoleType.Assistant },
                },
            },
        };

        _mockHttp
            .When(HttpMethod.Post, $"{_clientBaseUri}v1/chat/completions")
            .Respond(JsonContent.Create(responseContent, options: JsonObjectSerializer.SnakeCaseOptions));

        // Act
        var result = await _clientStrategy.GetCompletionAsync(chatItems);

        // Assert
        result.Should().Be("Hello, how can I assist you?");
    }

    [Fact]
    public async Task GetCompletionAsync_ShouldThrowException_WhenResponseIsUnsuccessful()
    {
        // Arrange
        var chatItems = new List<ChatItem> { new(Role: RoleType.User, Prompt: "Hello") };

        _mockHttp.When(HttpMethod.Post, $"{_clientBaseUri}v1/chat/completions").Respond(HttpStatusCode.BadRequest);

        // Act
        Func<Task> act = async () => await _clientStrategy.GetCompletionAsync(chatItems);

        // Assert
        var exception = await act.Should().ThrowAsync<HttpRequestException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetEmbeddingAsync_ShouldReturnEmbedding_WhenResponseIsSuccessful()
    {
        // Arrange
        var input = "test input";

        var responseContent = new LlamaEmbeddingResponse
        {
            Data = new List<LlamaEmbeddingData>
            {
                new()
                {
                    Embedding = new List<double> { 0.1, 0.2, 0.3 },
                },
            },
        };

        _mockHttp
            .When(HttpMethod.Post, $"{_clientBaseUri}v1/embeddings")
            .Respond(JsonContent.Create(responseContent, options: JsonObjectSerializer.SnakeCaseOptions));

        // Act
        var result = await _clientStrategy.GetEmbeddingAsync(input);

        // Assert
        result.Should().BeEquivalentTo(new List<double> { 0.1, 0.2, 0.3 });
    }

    [Fact]
    public async Task GetEmbeddingAsync_ShouldReturnEmptyList_WhenResponseIsUnsuccessful()
    {
        // Arrange
        var input = "test input";

        _mockHttp.When(HttpMethod.Post, $"{_clientBaseUri}v1/embeddings").Respond(HttpStatusCode.BadRequest);

        // Act
        Func<Task> act = async () => await _clientStrategy.GetEmbeddingAsync(input);

        // Assert
        var exception = await act.Should().ThrowAsync<HttpRequestException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCompletionStreamAsync_ShouldReturnMessages()
    {
        // Arrange
        var chatItems = new List<ChatItem> { new(Role: RoleType.User, Prompt: "Hello, how are you?") };

        var streamingResponse = new StringBuilder();
        streamingResponse.Append("data: {\"choices\":[{\"delta\":{\"role\":\"assistant\",\"content\":\"Hello\"}}]}\n");
        streamingResponse.Append("data: {\"choices\":[{\"delta\":{\"role\":\"assistant\",\"content\":\" world\"}}]}\n");
        streamingResponse.Append("data: [DONE]\n");

        _mockHttp
            .When(HttpMethod.Post, $"{_clientBaseUri}v1/chat/completions")
            .Respond("text/event-stream", streamingResponse.ToString());

        // Act
        var messages = await _clientStrategy.GetCompletionStreamAsync(chatItems).ToListAsync();

        // Assert
        messages.Count.Should().Be(2);
        messages[0].Should().Be("Hello");
        messages[1].Should().Be(" world");
    }

    [Fact]
    public async Task GetCompletionStreamAsync_ShouldHandleEmptyResponse()
    {
        // Arrange
        var chatItems = new List<ChatItem> { new(Role: RoleType.User, Prompt: "What's up?") };

        var emptyResponse = "data: [DONE]\n";

        _mockHttp
            .When(HttpMethod.Post, $"{_clientBaseUri}v1/chat/completions")
            .Respond("text/event-stream", emptyResponse);

        // Act
        var result = _clientStrategy.GetCompletionStreamAsync(chatItems).ToListAsync();

        // Assert
        var messages = await result;
        messages.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCompletionStreamAsync_ShouldThrowException_OnFailedRequest()
    {
        // Arrange
        var chatItems = new List<ChatItem> { new(Role: RoleType.User, Prompt: "Tell me a joke.") };

        _mockHttp
            .When(HttpMethod.Post, $"{_clientBaseUri}v1/chat/completions")
            .Respond(HttpStatusCode.InternalServerError);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await foreach (var message in _clientStrategy.GetCompletionStreamAsync(chatItems))
            {
                // no messages should be yielded
            }
        });
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
