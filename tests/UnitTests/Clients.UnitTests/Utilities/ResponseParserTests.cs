using Clients.Utilities;
using FluentAssertions;

namespace Clients.UnitTests.Utilities;

public class ResponseParserTests
{
    [Fact]
    public void GetCodeChangesFromResponse_ShouldReturnJson_WhenValidJsonBlockIsPresent()
    {
        // Arrange
        string input =
            @"# Final Result

        ```json
        {
            ""codeChanges"": [
                {
                    ""fileRelativePath"": ""path of modified or new code."",
                    ""code"": ""The modified or new code.""
                }]
        }
        ```";

        // Act
        var result = ResponseParser.GetCodeChangesFromResponse(input);

        // Assert
        result.Should().NotBeNull();

        // Assert that 'codeChanges' is not empty
        result.CodeChanges.Should().NotBeEmpty();

        // Assert that there is exactly 1 code change
        result.CodeChanges.Count.Should().Be(1);

        // Assert specific values of the first code change
        var firstCodeChange = result.CodeChanges.First();
        firstCodeChange.FileRelativePath.Should().Be("path of modified or new code.");
        firstCodeChange.Code.Should().Be("The modified or new code.");

        // Optional: Assert that the file path is not null or empty
        firstCodeChange.FileRelativePath.Should().NotBeNullOrWhiteSpace();

        // Optional: Assert that the code content contains a specific word or pattern
        firstCodeChange.Code.Should().Contain("modified or new");
    }

    [Fact]
    public void GetCodeChangesFromResponse_ShouldReturnNull_WhenNoFinalResultSectionIsPresent()
    {
        // Arrange
        string input =
            @"# Some Other Section

    ```json
    {
        ""codeChanges"": [
            {
                ""fileRelativePath"": ""path of modified or new code."",
                ""code"": ""The modified or new code.""
            }
        ]
    }
    ```";

        // Act
        var result = ResponseParser.GetCodeChangesFromResponse(input);

        // Assert
        result.Should().BeNull(); // Expect null because # Final Result section is missing
    }

    [Fact]
    public void GetCodeChangesFromResponse_ShouldThrowJsonException_WhenInvalidJsonBlockIsPresent()
    {
        // Arrange
        string input =
            @"# Final Result

    ```json
    {
        ""codeChanges"": [
            {
                ""fileRelativePath"": ""path of modified or new code."",
                ""code"": ""The modified or new code."",
            }  // Trailing comma causes this to be invalid JSON
        ]
    }
    ```";

        // Act & Assert
        Action act = () => ResponseParser.GetCodeChangesFromResponse(input);

        act.Should()
            .Throw<System.Text.Json.JsonException>()
            .WithMessage("*The JSON object contains a trailing comma at the end which is not supported in this mode.*");
    }

    [Fact]
    public void GetCodeChangesFromResponse_ShouldReturnEmptyCodeChanges_WhenJsonBlockIsEmpty()
    {
        // Arrange
        string input =
            @"# Final Result

    ```json
    {
        ""codeChanges"": []
    }
    ```";

        // Act
        var result = ResponseParser.GetCodeChangesFromResponse(input);

        // Assert
        result.Should().NotBeNull();
        result.CodeChanges.Should().BeEmpty();
    }

    [Fact]
    public void GetCodeChangesFromResponse_ShouldReturnMultipleCodeChanges_WhenMultipleEntriesArePresent()
    {
        // Arrange
        string input =
            @"# Final Result

    ```json
    {
        ""codeChanges"": [
            {
                ""fileRelativePath"": ""path1.cs"",
                ""code"": ""code1""
            },
            {
                ""fileRelativePath"": ""path2.cs"",
                ""code"": ""code2""
            }
        ]
    }
    ```";

        // Act
        var result = ResponseParser.GetCodeChangesFromResponse(input);

        // Assert
        result.Should().NotBeNull();
        result.CodeChanges.Should().HaveCount(2);

        // Assert specific values for both code changes
        var firstCodeChange = result.CodeChanges[0];
        firstCodeChange.FileRelativePath.Should().Be("path1.cs");
        firstCodeChange.Code.Should().Be("code1");

        var secondCodeChange = result.CodeChanges[1];
        secondCodeChange.FileRelativePath.Should().Be("path2.cs");
        secondCodeChange.Code.Should().Be("code2");
    }

    [Fact]
    public void GetCodeChangesFromResponse_ShouldHandleMissingPropertiesGracefully()
    {
        // Arrange
        string input =
            @"# Final Result

    ```json
    {
        ""codeChanges"": [
            {
                ""code"": ""The modified code with missing fileRelativePath.""
            }
        ]
    }
    ```";

        // Act
        var result = ResponseParser.GetCodeChangesFromResponse(input);

        // Assert
        result.Should().NotBeNull();
        result.CodeChanges.Should().HaveCount(1);

        var codeChange = result.CodeChanges.First();

        // Assert that code is correct
        codeChange.Code.Should().Be("The modified code with missing fileRelativePath.");

        // Assert that FileRelativePath is null or empty because it was missing in JSON
        codeChange.FileRelativePath.Should().BeNull();
    }

    [Fact]
    public void GetCodeChangesFromResponse_ShouldReturnNull_WhenJsonBlockIsMissing()
    {
        // Arrange
        string input =
            @"# Some Other Section

    Here is some other content that does not contain the expected JSON block.";

        // Act
        var result = ResponseParser.GetCodeChangesFromResponse(input);

        // Assert
        result.Should().BeNull(); // Expect null because the JSON block is missing
    }

    [Fact]
    public void GetCodeChangesFromResponse_ShouldIgnoreTextAfterJsonBlock_WhenValidJsonBlockIsPresent()
    {
        // Arrange
        string input =
            @"# Final Result

    ```json
    {
        ""codeChanges"": [
            {
                ""fileRelativePath"": ""Models/Divide.cs"",
                ""code"": ""The modified or new code.""
            }]
    }
    ```

    This is some extra text after the JSON code block that should not affect the result.";

        // Act
        var result = ResponseParser.GetCodeChangesFromResponse(input);

        // Assert
        result.Should().NotBeNull();

        // Assert that 'codeChanges' is not empty
        result.CodeChanges.Should().NotBeEmpty();

        // Assert that there is exactly 1 code change
        result.CodeChanges.Count.Should().Be(1);

        // Assert specific values of the first code change
        var firstCodeChange = result.CodeChanges.First();
        firstCodeChange.FileRelativePath.Should().Be("Models/Divide.cs");
        firstCodeChange.Code.Should().Be("The modified or new code.");
    }
}
