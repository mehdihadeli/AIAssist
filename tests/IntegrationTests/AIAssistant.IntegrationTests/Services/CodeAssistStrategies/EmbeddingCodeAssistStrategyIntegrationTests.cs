using AIAssistant.Contracts;
using AIAssistant.Contracts.CodeAssist;
using AIAssistant.Models;
using Clients.Chat.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TreeSitter.Bindings.UnitTests.TestData;

namespace AIAssistant.IntegrationTests.Services.CodeAssistStrategies;

[Collection(ApplicationCollection.Name)]
public class EmbeddingCodeAssistStrategyIntegrationTests(ApplicationFixture applicationFixture) : IAsyncLifetime
{
    private IHost _app = default!;
    private ICodeAssist _codeAssist = default!;
    private string _appWorkingDir = default!;
    private string _originalWorkingDir = default!;

    [Fact]
    public async Task QueryAsync_With_A_ContextWorkingDirectory_ShouldReturnResponsesFromLLM()
    {
        // Arrange
        var userQuery = "can you remove all comments in Add.cs file?";
        await _codeAssist.LoadCodeFiles(new ChatSession(), contextWorkingDirectory: _appWorkingDir, codeFiles: null);

        // Act
        IAsyncEnumerable<string?> responseStream = _codeAssist.QueryAsync(userQuery);
        var response = await responseStream.ToListAsync();

        // Assert
        response.Should().NotBeNull();
        response.Should().NotBeEmpty();
        response.Any(s => !string.IsNullOrEmpty(s)).Should().BeTrue();
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
        await _codeAssist.LoadCodeFiles(new ChatSession(), contextWorkingDirectory: null, codeFiles: files);

        // Act
        IAsyncEnumerable<string?> responseStream = _codeAssist.QueryAsync(userQuery);
        var response = await responseStream.ToListAsync();

        // Assert
        response.Should().NotBeNull();
        response.Should().NotBeEmpty();
        response.Any(s => !string.IsNullOrEmpty(s)).Should().BeTrue();
    }

    public Task InitializeAsync()
    {
        _app = applicationFixture.App;
        _originalWorkingDir = Directory.GetCurrentDirectory();

        // Save the original working directory
        _appWorkingDir = Path.Combine(Directory.GetCurrentDirectory(), "TestData/Calculator");

        // Change the working directory to the new test directory
        Directory.SetCurrentDirectory(_appWorkingDir);

        var codeAssistStrategyFactory = _app.Services.GetRequiredService<ICodeAssistFactory>();
        _codeAssist = codeAssistStrategyFactory.Create(CodeAssistType.Embedding);

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Directory.SetCurrentDirectory(_originalWorkingDir);

        return Task.CompletedTask;
    }
}
