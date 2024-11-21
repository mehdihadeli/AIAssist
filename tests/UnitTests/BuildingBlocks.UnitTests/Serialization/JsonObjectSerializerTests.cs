using BuildingBlocks.Serialization;
using FluentAssertions;
using Xunit;

namespace BuildingBlocks.UnitTests.Serialization;

public class JsonObjectSerializerTests
{
    private readonly JsonObjectSerializer _serializer = new(JsonObjectSerializer.SnakeCaseOptions);

    [Fact]
    public void Serialize_ShouldReturnJsonString_WhenGivenValidObject()
    {
        // Arrange
        var response = new ChatCompletionResponse
        {
            Id = "chatcmpl-123",
            Object = "chat.completion",
            Created = 1677652288,
            Model = "gpt-4o-mini",
            SystemFingerprint = "fp_44709d6fcb",
            Choices =
            [
                new Choice
                {
                    Index = 0,
                    Message = new Message
                    {
                        Role = RoleType.Assistant,
                        Content = "\n\nHello there, how may I assist you today?",
                    },
                    Logprobs = null,
                    FinishReason = "stop",
                },
            ],
            Usage = new Usage
            {
                PromptTokens = 9,
                CompletionTokens = 12,
                TotalTokens = 21,
                CompletionTokensDetails = new CompletionTokensDetails { ReasoningTokens = 0 },
            },
        };

        // Act
        var json = _serializer.Serialize(response);

        // Assert
        json.Should().NotBeNull();
        json.Should().Contain("\"id\": \"chatcmpl-123\"");
        json.Should().Contain("\"object\": \"chat.completion\"");
        json.Should().Contain("\"model\": \"gpt-4o-mini\"");
        // our serializer uses `PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower` during its process and add `_` for multi-word names
        json.Should().Contain("\"system_fingerprint\": \"fp_44709d6fcb\"");
    }

    [Fact]
    public void Deserialize_ShouldReturnObject_WhenGivenValidJson()
    {
        // Arrange
        var json =
            @"
        {
          ""id"": ""chatcmpl-123"",
          ""object"": ""chat.completion"",
          ""created"": 1677652288,
          ""model"": ""gpt-4o-mini"",
          ""system_fingerprint"": ""fp_44709d6fcb"",
          ""choices"": [{
            ""index"": 0,
            ""message"": {
              ""role"": ""assistant"",
              ""content"": ""\\n\\nHello there, how may I assist you today?""
            },
            ""logprobs"": null,
            ""finish_reason"": ""stop""
          }],
          ""usage"": {
            ""prompt_tokens"": 9,
            ""completion_tokens"": 12,
            ""total_tokens"": 21,
            ""completion_tokens_details"": {
              ""reasoning_tokens"": 0
            }
          }
        }";

        // Act
        var response = _serializer.Deserialize<ChatCompletionResponse>(json);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be("chatcmpl-123");
        response.Object.Should().Be("chat.completion");
        response.Model.Should().Be("gpt-4o-mini");
        response.Choices.Should().HaveCount(1);
        response.Choices[0].Message.Role.Should().Be(RoleType.Assistant);
        response.Choices[0].Message.Content.Should().Contain("Hello there, how may I assist you today?");
    }

    [Fact]
    public void Deserialize_ShouldThrowJsonException_WhenGivenInvalidJson()
    {
        // Arrange
        var invalidJson = "{ this is not valid JSON }";

        // Act
        var act = () => _serializer.Deserialize<ChatCompletionResponse>(invalidJson);

        // Assert
        act.Should().Throw<System.Text.Json.JsonException>();
    }
}
