using BuildingBlocks.LLM;
using Microsoft.ML.Tokenizers;
using NSubstitute;
using Xunit;

namespace BuildingBlocks.UnitTests.LLM;

public class TokenizerHelperTests
{
    [Fact]
    public void GPT4TokenCount_ShouldReturnCorrectTokenCount()
    {
        // Arrange
        var prompt = "This is a test prompt";

        // Act
        var tokenCount = TokenizerHelper.GPT4TokenCount(prompt);
        var tokenCount2 = TokenizerHelper.TokenCount(prompt);

        // Assert
        Assert.Equal(5, tokenCount);
    }

    [Fact]
    public void TokenCount_ShouldReturnCorrectTokenCount()
    {
        // Arrange
        var prompt = "This is a test prompt";

        // Act
        var tokenCount = TokenizerHelper.TokenCount(prompt);

        // Assert
        Assert.Equal(5, tokenCount);
    }

    [Fact]
    public void GPT4VectorTokens_ShouldReturnCorrectVectorTokens()
    {
        // Arrange
        var prompt = "This is a test prompt";

        // Act
        var tokenVector = TokenizerHelper.GPT4VectorTokens(prompt);

        // Assert
        Assert.NotEmpty(tokenVector);
    }

    [Fact]
    public async Task LLama3_1TokenCount_ShouldReturnCorrectTokenCount()
    {
        // Arrange
        var prompt = "This is a test prompt";

        // Act
        var tokenCount = await TokenizerHelper.PhiTokenCount(prompt);

        // Assert
        Assert.Equal(6, tokenCount);
    }

    [Fact]
    public async Task LLama3_1VectorTokens_ShouldReturnCorrectVectorTokens()
    {
        // Arrange
        var prompt = "This is a test prompt";

        // Act
        var tokenVector = await TokenizerHelper.PhiVectorTokens(prompt);

        // Assert
        Assert.NotEmpty(tokenVector);
    }
}
