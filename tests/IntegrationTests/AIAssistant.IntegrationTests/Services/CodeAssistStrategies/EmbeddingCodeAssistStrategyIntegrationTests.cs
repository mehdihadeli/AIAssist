using AIAssistant.Contracts;
using AIAssistant.Models;
using Clients.Chat.Models;
using Clients.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AIAssistant.IntegrationTests.Services.CodeAssistStrategies;

[Collection(ApplicationCollection.Name)]
public class EmbeddingCodeAssistStrategyIntegrationTests(ApplicationFixture applicationFixture) : IAsyncLifetime
{
    private IHost _app = default!;
    private ICodeStrategy _codeStrategy = default!;
    private string _appWorkingDir = default!;
    private string _originalWorkingDir = default!;

    [Fact]
    public async Task QueryAsync_With_A_ContextWorkingDirectory_ShouldReturnResponsesFromLLM()
    {
        // Arrange
        var userQuery = "can you give me all method names in Add.cs file?";
        await _codeStrategy.LoadCodeFiles(new ChatSession(), contextWorkingDirectory: _appWorkingDir);

        // Act
        IAsyncEnumerable<string?> responseStream = _codeStrategy.QueryAsync(userQuery);

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

    public Task InitializeAsync()
    {
        _originalWorkingDir = Directory.GetCurrentDirectory();

        // Save the original working directory
        _appWorkingDir = Path.Combine(Directory.GetCurrentDirectory(), "TestData/Calculator");

        // Change the working directory to the new test directory
        Directory.SetCurrentDirectory(_appWorkingDir);

        _app = applicationFixture.App;

        var codeAssistStrategyFactory = _app.Services.GetRequiredService<ICodeAssistStrategyFactory>();
        _codeStrategy = codeAssistStrategyFactory.Create(CodeAssistStrategyType.Embedding);

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Directory.SetCurrentDirectory(_originalWorkingDir);
        return Task.CompletedTask;
    }
}
