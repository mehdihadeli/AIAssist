using System.Text;
using AIAssist.Contracts;
using AIAssist.Contracts.CodeAssist;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using TreeSitter.Bindings.UnitTests.TestData;

namespace AIAssistant.IntegrationTests.Services;

[Collection(ApplicationCollection.Name)]
public class CodeAssistantManagerIntegrationTests(ApplicationFixture applicationFixture) : IAsyncLifetime
{
    private IHost _app = default!;
    private ICodeAssistantManager _codeAssistantManager = default!;
    private IChatSessionManager _chatSessionManager = default!;
    private string _appWorkingDir = default!;
    private string _originalWorkingDir = default!;

    [Fact]
    public async Task QueryAsync_With_A_ContextWorkingDirectory_ShouldReturnResponsesFromLLM()
    {
        // Arrange
        var userQuery = "can you remove all comments in Add.cs and Add class?";
        await _codeAssistantManager.LoadCodeFiles(contextWorkingDirectory: _appWorkingDir, codeFiles: null);

        // Act
        IAsyncEnumerable<string?> responseStream = _codeAssistantManager.QueryAsync(userQuery);

        // Collect responses from the stream
        var responses = new List<string?>();
        await foreach (var response in responseStream)
        {
            responses.Add(response);
        }

        // Assert
        Assert.NotNull(responses);
        Assert.NotEmpty(responses);
        Assert.All(responses, response => Assert.False(string.IsNullOrWhiteSpace(response)));
    }

    [Fact]
    public async Task QueryAsync_With_Some_Files_ShouldReturnResponsesFromLLM()
    {
        // Arrange
        var userQuery = "can you remove all comments in Add.cs file?";
        string[] files =
        [
            TestDataConstants.CalculatorApp.AddRelativeFilePath,
            TestDataConstants.CalculatorApp.SubtractRelativeFilePath,
            TestDataConstants.CalculatorApp.DivideRelativeFilePath,
            TestDataConstants.CalculatorApp.MultiplyRelativeFilePath,
        ];
        await _codeAssistantManager.LoadCodeFiles(contextWorkingDirectory: null, codeFiles: files);

        // Act
        IAsyncEnumerable<string?> responseStream = _codeAssistantManager.QueryAsync(userQuery);
        var response = await CollectStreamResponseAsync(responseStream);
        var changesCodeBlocks = _codeAssistantManager.ParseResponseCodeBlocks(response);

        // Assert
        changesCodeBlocks.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyChanges_With_A_ContextWorkingDirectory_ShouldApplyResponseFromLLM()
    {
        // Arrange
        var userQuery = "can you remove all comments in Add.cs file?";
        await _codeAssistantManager.LoadCodeFiles(contextWorkingDirectory: _appWorkingDir, codeFiles: null);

        // Act
        IAsyncEnumerable<string?> responseStream = _codeAssistantManager.QueryAsync(userQuery);
        var response = await CollectStreamResponseAsync(responseStream);
        var changesCodeBlocks = _codeAssistantManager.ParseResponseCodeBlocks(response);

        // Assert
        changesCodeBlocks.Should().NotBeNull();
    }

    public Task InitializeAsync()
    {
        _app = applicationFixture.App;

        _originalWorkingDir = Directory.GetCurrentDirectory();

        // Save the original working directory
        _appWorkingDir = Path.Combine(Directory.GetCurrentDirectory(), "TestData/Calculator");

        // Change the working directory to the new test directory
        Directory.SetCurrentDirectory(_appWorkingDir);

        _codeAssistantManager = _app.Services.GetRequiredService<ICodeAssistantManager>();
        _chatSessionManager = _app.Services.GetRequiredService<IChatSessionManager>();
        var session = _chatSessionManager.CreateNewSession();
        _chatSessionManager.SetCurrentActiveSession(session);

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _chatSessionManager.SetCurrentActiveSession(null);

        Directory.SetCurrentDirectory(_originalWorkingDir);

        return Task.CompletedTask;
    }

    public async Task<string> CollectStreamResponseAsync(IAsyncEnumerable<string?> responseStreams)
    {
        var completeResponse = new StringBuilder();

        // Collect and display each line from the response stream
        await foreach (var responseStream in responseStreams)
        {
            if (responseStream != null)
            {
                if (string.IsNullOrEmpty(responseStream))
                    continue;

                completeResponse.Append(responseStream);
            }
        }

        // Return the complete collected response
        return completeResponse.ToString();
    }
}
