using AIAssistant.Data;
using AIAssistant.Models;
using AIAssistant.Services;
using BuildingBlocks.Utils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AIAssistant.IntegrationTests.Services;

[Collection(ApplicationCollection.Name)]
public class EmbeddingServiceIntegrationTests(ApplicationFixture applicationFixture)
{
    private readonly IHost _app = applicationFixture.App;
    private readonly string _programFile = FilesUtilities.RenderTemplate("Calculator", "Program.cs", null);
    private readonly string _calculatorCsprojFile = FilesUtilities.RenderTemplate(
        "Calculator",
        "Calculator.csproj",
        null
    );
    private readonly string _operationFile = FilesUtilities.RenderTemplate("Calculator", "IOperation.cs", null);
    private readonly string _addFile = FilesUtilities.RenderTemplate("Calculator/Models", "Add.cs", null);
    private readonly string _divideFile = FilesUtilities.RenderTemplate("Calculator/Models", "Divide.cs", null);
    private readonly string _multiplyFile = FilesUtilities.RenderTemplate("Calculator/Models", "Multiply.cs", null);
    private readonly string _subtractFile = FilesUtilities.RenderTemplate("Calculator/Models", "Subtract.cs", null);

    [Fact]
    public async Task AddEmbeddingsForFiles_ShouldStoreEmbeddingsCorrectly()
    {
        // Arrange
        var embeddingService = _app.Services.GetRequiredService<EmbeddingService>();
        var sessionId = Guid.NewGuid();

        var applicationCodes = new List<ApplicationCode>
        {
            new(Code: _addFile, RelativePath: "Calculator/Models/Add.cs"),
            new(_subtractFile, "Calculator/Models/Subtract.cs"),
            new(_multiplyFile, "Calculator/Models/Multiply.cs"),
            new(_divideFile, "Calculator/Models/Divide.cs"),
            new(_programFile, "Calculator/Program.cs"),
            new(_operationFile, "Calculator/IOperation.cs"),
            new(_calculatorCsprojFile, "Calculator/Calculator.csproj"),
        };

        // Act: Add embeddings for the files
        await embeddingService.AddEmbeddingsForFiles(applicationCodes, sessionId);

        // Assert: Check if embeddings were stored correctly
        var userRequest = "can you remove all comments in the classes";
        var userQueryEmbeddings = await embeddingService.GenerateEmbeddingForUserInput(userRequest);
        var embeddingsStore = _app.Services.GetRequiredService<EmbeddingsStore>();
        var storedEmbeddings = embeddingsStore.Query(userQueryEmbeddings, sessionId).ToList();

        // Use FluentAssertions for assertions
        storedEmbeddings.Should().NotBeNull();
        storedEmbeddings.Should().HaveCount(4); // Expecting 4 embeddings

        // Ensure that we have entries for all our application codes
        storedEmbeddings.Should().Contain(e => e.RelativeFilePath == "Add.cs");
        storedEmbeddings.Should().Contain(e => e.RelativeFilePath == "Subtract.cs");
        storedEmbeddings.Should().Contain(e => e.RelativeFilePath == "Multiply.cs");
        storedEmbeddings.Should().Contain(e => e.RelativeFilePath == "Divide.cs");
    }

    [Fact]
    public async Task GetRelatedEmbeddings_ShouldReturnRelevantEmbeddings()
    {
        // Arrange
        var embeddingService = _app.Services.GetRequiredService<EmbeddingService>();
        var sessionId = Guid.NewGuid();

        var applicationCodes = new List<ApplicationCode>
        {
            new(Code: _addFile, RelativePath: "Calculator/Models/Add.cs"),
            new(_subtractFile, "Calculator/Models/Subtract.cs"),
            new(_multiplyFile, "Calculator/Models/Multiply.cs"),
            new(_divideFile, "Calculator/Models/Divide.cs"),
            new(_programFile, "Calculator/Program.cs"),
            new(_operationFile, "Calculator/IOperation.cs"),
            new(_calculatorCsprojFile, "Calculator/Calculator.csproj"),
        };

        // Add some embeddings to the store
        await embeddingService.AddEmbeddingsForFiles(applicationCodes, sessionId);

        // Act: Query for relevant embeddings
        var userRequest = "Can you give me all method names inside Divide class?";
        var relevantEmbeddings = (await embeddingService.GetRelatedEmbeddings(userRequest, sessionId)).ToList();

        // Assert: Check that relevant embeddings are returned
        relevantEmbeddings.Should().NotBeNull();
        relevantEmbeddings.Should().HaveCount(1);
        relevantEmbeddings.Should().Contain(e => e.RelativeFilePath == "Divide.cs");
    }
}
