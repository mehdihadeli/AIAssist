// using AIAssistant.Data;
// using AIAssistant.Models;
// using AIAssistant.Services;
// using BuildingBlocks.Utils;
// using FluentAssertions;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using TreeSitter.Bindings.CustomTypes.TreeParser;
//
// namespace AIAssistant.IntegrationTests.Services;
//
// [Collection(ApplicationCollection.Name)]
// public class EmbeddingServiceIntegrationTests(ApplicationFixture applicationFixture) : IAsyncLifetime
// {
//     private readonly IHost _app = applicationFixture.App;
//
//     private string _appWorkingDir = default!;
//     private string _originalWorkingDir = default!;
//
//     private string _programFile = default!;
//     private string _calculatorCsprojFile = default!;
//     private string _operationFile = default!;
//     private string _addFile = default!;
//     private string _divideFile = default!;
//     private string _multiplyFile = default!;
//     private string _subtractFile = default!;
//
//     private readonly string _addRelativeFilePath = "Calculator/Models/Add.cs";
//     private readonly string _subtractRelativeFilePath = "Calculator/Models/Subtract.cs";
//     private readonly string _multiplyRelativeFilePath = "Calculator/Models/Multiply.cs";
//     private readonly string _divideRelativeFilePath = "Calculator/Models/Divide.cs";
//     private readonly string _programRelativeFilePath = "Calculator/Program.cs";
//     private readonly string _operationRelativeFilePath = "Calculator/IOperation.cs";
//     private readonly string _csprojRelativeFilePath = "Calculator/Calculator.csproj";
//
//     public async Task InitializeAsync()
//     {
//         _originalWorkingDir = Directory.GetCurrentDirectory();
//
//         // Save the original working directory
//         _appWorkingDir = Path.Combine(Directory.GetCurrentDirectory(), "TestData/Calculator");
//
//         // Change the working directory to the new test directory
//         Directory.SetCurrentDirectory(_appWorkingDir);
//
//         _programFile = FilesUtilities.RenderTemplate("Calculator", "Program.cs", null);
//         _calculatorCsprojFile = FilesUtilities.RenderTemplate("Calculator", "Calculator.csproj", null);
//         _operationFile = FilesUtilities.RenderTemplate("Calculator", "IOperation.cs", null);
//         _addFile = FilesUtilities.RenderTemplate("Calculator/Models", "Add.cs", null);
//         _divideFile = FilesUtilities.RenderTemplate("Calculator/Models", "Divide.cs", null);
//         _multiplyFile = FilesUtilities.RenderTemplate("Calculator/Models", "Multiply.cs", null);
//         _subtractFile = FilesUtilities.RenderTemplate("Calculator/Models", "Subtract.cs", null);
//
//         await Task.CompletedTask;
//     }
//
//     public Task DisposeAsync()
//     {
//         Directory.SetCurrentDirectory(_originalWorkingDir);
//         return Task.CompletedTask;
//     }
//
//     [Fact]
//     public async Task AddEmbeddingsForFiles_ShouldStoreEmbeddingsCorrectly()
//     {
//         // Arrange
//         var embeddingService = _app.Services.GetRequiredService<EmbeddingService>();
//         var sessionId = Guid.NewGuid();
//
//         var applicationCodes = new List<CodeFile>
//         {
//             new(Code: _addFile, RelativePath: _addRelativeFilePath),
//             new(Code: _subtractFile, RelativePath: _subtractRelativeFilePath),
//             new(Code: _multiplyFile, RelativePath: _multiplyRelativeFilePath),
//             new(Code: _divideFile, RelativePath: _divideRelativeFilePath),
//             new(Code: _programFile, RelativePath: _programRelativeFilePath),
//             new(Code: _operationFile, RelativePath: _operationRelativeFilePath),
//             new(Code: _calculatorCsprojFile, RelativePath: _csprojRelativeFilePath),
//         };
//
//         // Act: Add embeddings for the files
//         await embeddingService.AddEmbeddingsForFiles(applicationCodes, sessionId);
//
//         // Assert: Check if embeddings were stored correctly
//         var userRequest = "can you remove all comments in the classes";
//         var userQueryEmbeddings = await embeddingService.GenerateEmbeddingForUserInput(userRequest);
//         var embeddingsStore = _app.Services.GetRequiredService<EmbeddingsStore>();
//         var storedEmbeddings = embeddingsStore.Query(userQueryEmbeddings, sessionId).ToList();
//
//         // Use FluentAssertions for assertions
//         storedEmbeddings.Should().NotBeNull();
//         storedEmbeddings.Should().HaveCountGreaterThanOrEqualTo(4);
//
//         // Ensure that we have entries for all our application codes
//         storedEmbeddings.Should().Contain(e => e.RelativeFilePath == _addRelativeFilePath);
//         storedEmbeddings.Should().Contain(e => e.RelativeFilePath == _subtractRelativeFilePath);
//         storedEmbeddings.Should().Contain(e => e.RelativeFilePath == _multiplyRelativeFilePath);
//         storedEmbeddings.Should().Contain(e => e.RelativeFilePath == _divideRelativeFilePath);
//     }
//
//     [Fact]
//     public async Task GetRelatedEmbeddings_ShouldReturnRelevantEmbeddings()
//     {
//         // Arrange
//         var embeddingService = _app.Services.GetRequiredService<EmbeddingService>();
//         var sessionId = Guid.NewGuid();
//
//         var applicationCodes = new List<CodeFile>
//         {
//             new(Code: _addFile, RelativePath: _addRelativeFilePath),
//             new(Code: _subtractFile, RelativePath: _subtractRelativeFilePath),
//             new(Code: _multiplyFile, RelativePath: _multiplyRelativeFilePath),
//             new(Code: _divideFile, RelativePath: _divideRelativeFilePath),
//             new(Code: _programFile, RelativePath: _programRelativeFilePath),
//             new(Code: _operationFile, RelativePath: _operationRelativeFilePath),
//             new(Code: _calculatorCsprojFile, RelativePath: _csprojRelativeFilePath),
//         };
//
//         // Add some embeddings to the store
//         await embeddingService.AddEmbeddingsForFiles(applicationCodes, sessionId);
//
//         // Act: Query for relevant embeddings
//         var userRequest = "Can you give me all method names inside of Divide class?";
//         var relevantEmbeddings = (await embeddingService.GetRelatedEmbeddings(userRequest, sessionId)).ToList();
//
//         // Assert: Check that relevant embeddings are returned
//         relevantEmbeddings.Should().NotBeNull();
//         relevantEmbeddings.Should().HaveCountGreaterThanOrEqualTo(1);
//         relevantEmbeddings.Should().Contain(e => e.RelativeFilePath == _divideRelativeFilePath);
//     }
//
//     [Fact]
//     public async Task GenerateEmbeddingForUserInput_ShouldReturnValidEmbedding()
//     {
//         // Arrange
//         var embeddingService = _app.Services.GetRequiredService<EmbeddingService>();
//         var userInput = "What is the sum of two numbers?";
//
//         // Act
//         var embedding = await embeddingService.GenerateEmbeddingForUserInput(userInput);
//
//         // Assert: Check that the embedding is not null and contains data
//         embedding.Should().NotBeNull();
//         embedding.Should().HaveCountGreaterThan(0);
//     }
//
//     [Fact]
//     public void CreateLLMContext_ShouldReturnCorrectFormattedString()
//     {
//         // Arrange
//         var embeddingService = _app.Services.GetRequiredService<EmbeddingService>();
//
//         var relevantCode = new List<CodeEmbedding>
//         {
//             new() { RelativeFilePath = _addRelativeFilePath, Code = _addFile },
//             new() { RelativeFilePath = _subtractRelativeFilePath, Code = _subtractFile },
//         };
//
//         // Act
//         var contextString = embeddingService.CreateLLMContext(relevantCode);
//
//         // Assert: Verify the output string contains the expected content
//         contextString.Should().NotBeNullOrEmpty();
//         contextString.Should().Contain("```csharp Add.cs");
//         contextString.Should().Contain("```csharp Subtract.cs");
//         contextString.Should().Contain("// Calculator/Models/Add.cs");
//         contextString.Should().Contain("// Calculator/Models/Subtract.cs");
//
//         contextString
//             .Should()
//             .Contain(
//                 @"// Calculator/Models/Add.cs
// ```csharp Add.cs
// namespace Calculator.Models;
// /// <summary>
// /// Add two value
// /// </summary>
// /// <param Name=""number1""></param>
// /// <param Name=""number2""></param>
// public class Add(double number1, double number2) : IOperation
// {
//     public double Calculate()
//     {
//         return AddNumbers();
//     }
//
//     private double AddNumbers()
//     {
//         return number1 / number2;
//     }
// }
// ```"
//             );
//         contextString
//             .Should()
//             .Contain(
//                 @"// Calculator/Models/Subtract.cs
// ```csharp Subtract.cs
// namespace Calculator.Models;
//
// /// <summary>
// /// Subtract two values
// /// </summary>
// /// <param Name=""number1""></param>
// /// <param Name=""number2""></param>
// public class Subtract(double number1, double number2) : IOperation
// {
//     public double Calculate()
//     {
//         return number1 - number2;
//     }
// }
// ```"
//             );
//     }
// }
