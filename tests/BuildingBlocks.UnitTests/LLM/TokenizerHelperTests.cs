using BuildingBlocks.LLM;
using Xunit;

namespace BuildingBlocks.UnitTests.LLM;

public class TokenizerHelperTests
{
    [Theory]
    [InlineData("gpt-3.5-turbo", "Hello, world!", 4)] // Replace 4 with the expected token count for this prompt and model
    [InlineData("gpt-3.5-turbo", "This is a test.", 5)] // Adjust expected counts as necessary
    [InlineData("gpt-3.5-turbo", "", 0)] // Test with an empty prompt
    public void GetTokenCount_ShouldReturnExpectedCount(string model, string prompt, int expectedCount)
    {
        // Act
        int actualCount = TokenizerHelper.TokenCount(model, prompt);

        // Assert
        Assert.Equal(expectedCount, actualCount);
    }

    [Theory]
    [InlineData("gpt-3.5-turbo", "Hello, world!", 4)] // Replace 4 with the expected token count for this prompt and model
    [InlineData("gpt-3.5-turbo", "This is a test.", 5)] // Adjust expected counts as necessary
    [InlineData("gpt-3.5-turbo", "", 0)] // Test with an empty prompt
    public void EncodeToTokens_ShouldReturnExpectedCount(string model, string prompt, int expectedCount)
    {
        // Act
        double[] actualCount = TokenizerHelper.CreateVectorTokens(model, prompt);
    }
}
