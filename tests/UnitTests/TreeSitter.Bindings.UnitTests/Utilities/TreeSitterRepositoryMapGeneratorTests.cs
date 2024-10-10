using BuildingBlocks.Utils;
using TreeSitter.Bindings.CustomTypes.TreeParser;
using TreeSitter.Bindings.Utilities;

namespace TreeSitter.Bindings.UnitTests.Utilities;

public class TreeSitterRepositoryMapGeneratorTests : IAsyncLifetime
{
    private string _programFile = default!;
    private string _calculatorCsprojFile = default!;
    private string _operationFile = default!;
    private string _addFile = default!;
    private string _divideFile = default!;
    private string _multiplyFile = default!;
    private string _subtractFile = default!;

    private readonly string _addRelativeFilePath = "Models/Add.cs";
    private readonly string _subtractRelativeFilePath = "Models/Subtract.cs";
    private readonly string _multiplyRelativeFilePath = "Models/Multiply.cs";
    private readonly string _divideRelativeFilePath = "Models/Divide.cs";
    private readonly string _programRelativeFilePath = "Program.cs";
    private readonly string _operationRelativeFilePath = "IOperation.cs";
    private readonly string _csprojRelativeFilePath = "Calculator.csproj";

    public async Task InitializeAsync()
    {
        _programFile = FilesUtilities.RenderTemplate("TestData/Calculator", "Program.cs", null);
        _calculatorCsprojFile = FilesUtilities.RenderTemplate("TestData/Calculator", "Calculator.csproj", null);
        _operationFile = FilesUtilities.RenderTemplate("TestData/Calculator", "IOperation.cs", null);
        _addFile = FilesUtilities.RenderTemplate("TestData/Calculator/Models", "Add.cs", null);
        _divideFile = FilesUtilities.RenderTemplate("TestData/Calculator/Models", "Divide.cs", null);
        _multiplyFile = FilesUtilities.RenderTemplate("TestData/Calculator/Models", "Multiply.cs", null);
        _subtractFile = FilesUtilities.RenderTemplate("TestData/Calculator/Models", "Subtract.cs", null);

        await Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeLevelString_ShouldGenerateCorrectTreeLevelString_ForAddFile()
    {
        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateSimpleTreeSitterRepositoryMap(
            _addFile,
            _addRelativeFilePath,
            new RepositoryMap()
        );

        // Assert for tree structure
        Assert.Contains("namespace Calculator", result, StringComparison.Ordinal); // Check namespace
        Assert.Contains("class Add", result, StringComparison.Ordinal); // Check class definition
        Assert.Contains("double Result { get; set; }", result, StringComparison.Ordinal); // Property
        Assert.Contains("double Calculate()", result, StringComparison.Ordinal); // Method
        Assert.Contains("double AddNumbers(double first, double second)", result, StringComparison.Ordinal); // Private method

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeLevelString_ShouldGenerateCorrectTreeLevelString_ForSubtractFile()
    {
        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateSimpleTreeSitterRepositoryMap(
            _subtractFile,
            _subtractRelativeFilePath,
            new RepositoryMap()
        );

        // Assert for tree structure
        Assert.Contains("namespace Calculator", result, StringComparison.Ordinal); // Check namespace
        Assert.Contains("class Subtract", result, StringComparison.Ordinal); // Check class definition
        Assert.Contains("double Result { get; set; }", result, StringComparison.Ordinal); // Property
        Assert.Contains("double Calculate()", result, StringComparison.Ordinal); // Method

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeLevelString_ShouldGenerateCorrectTreeLevelString_ForDivideFile()
    {
        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateSimpleTreeSitterRepositoryMap(
            _divideFile,
            _divideRelativeFilePath,
            new RepositoryMap()
        );

        // Assert for tree structure
        Assert.Contains("namespace Calculator", result, StringComparison.Ordinal); // Check namespace
        Assert.Contains("class Divide", result, StringComparison.Ordinal); // Check class definition
        Assert.Contains("double Result { get; set; }", result, StringComparison.Ordinal); // Property
        Assert.Contains("double Calculate()", result, StringComparison.Ordinal); // Method
        Assert.Contains("double DivideNumbers()", result, StringComparison.Ordinal); // Private method

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeLevelString_ShouldGenerateCorrectTreeLevelString_ForMultiplyFile()
    {
        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateSimpleTreeSitterRepositoryMap(
            _multiplyFile,
            _multiplyRelativeFilePath,
            new RepositoryMap()
        );

        // Assert for tree structure
        Assert.Contains("namespace Calculator", result, StringComparison.Ordinal); // Check namespace
        Assert.Contains("class Multiply", result, StringComparison.Ordinal); // Check class definition
        Assert.Contains("double Result { get; set; }", result, StringComparison.Ordinal); // Property
        Assert.Contains("double Calculate()", result, StringComparison.Ordinal); // Method

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeLevelString_ShouldGenerateCorrectTreeLevelString_ForProgramFile()
    {
        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateSimpleTreeSitterRepositoryMap(
            _programFile,
            _programRelativeFilePath,
            new RepositoryMap()
        );

        // Assert for tree structure
        Assert.Contains("namespace Calculator", result, StringComparison.Ordinal); // Check namespace
        Assert.Contains("class Program", result, StringComparison.Ordinal); // Check main class
        Assert.Contains("static void Main", result, StringComparison.Ordinal); // Main method
        Assert.Contains("double num1 =", result, StringComparison.Ordinal); // First number input
        Assert.Contains("double num2 =", result, StringComparison.Ordinal); // Second number input

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeLevelString_ShouldReturnEmpty_ForEmptyFile()
    {
        // Arrange
        string emptyFileContent = string.Empty;

        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateSimpleTreeSitterRepositoryMap(
            emptyFileContent,
            _subtractRelativeFilePath,
            new RepositoryMap()
        );

        // Assert
        Assert.Equal(string.Empty, result);

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeLevelString_ShouldMatchExpectedOutput_ForProjectStructure()
    {
        // Arrange
        var expected =
            @"root/
├── Calculator/
│   ├── Program.cs:
│   │   ├── namespace Calculator:
│   │   │   ├── class Program:
│   │   │   │   ├── void Main(string[] args)
├── Models/
│   ├── Add.cs:
│   │   ├── namespace Calculator:
│   │   │   ├── class Add:
│   │   │   │   ├── double Result { get; set; }
│   │   │   │   ├── double Calculate()
│   │   │   │   ├── private double AddNumbers(double first, double second)
│   ├── Subtract.cs:
│   │   ├── namespace Calculator:
│   │   │   ├── class Subtract:
│   │   │   │   ├── double Result { get; set; }
│   │   │   │   ├── double Calculate()
│   ├── Divide.cs:
│   │   ├── namespace Calculator:
│   │   │   ├── class Divide:
│   │   │   │   ├── double Result { get; set; }
│   │   │   │   ├── double Calculate()
│   │   │   │   ├── private double DivideNumbers()
│   ├── Multiply.cs:
│   │   ├── namespace Calculator:
│   │   │   ├── class Multiply:
│   │   │   │   ├── double Result { get; set; }
│   │   │   │   ├── double Calculate()
";

        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateSimpleTreeSitterRepositoryMap(
            _programFile,
            _programRelativeFilePath,
            new RepositoryMap()
        );
        result += TreeSitterRepositoryMapGenerator.GenerateSimpleTreeSitterRepositoryMap(
            _addFile,
            _addRelativeFilePath,
            new RepositoryMap()
        );
        result += TreeSitterRepositoryMapGenerator.GenerateSimpleTreeSitterRepositoryMap(
            _subtractFile,
            _subtractRelativeFilePath,
            new RepositoryMap()
        );
        result += TreeSitterRepositoryMapGenerator.GenerateSimpleTreeSitterRepositoryMap(
            _divideFile,
            _divideRelativeFilePath,
            new RepositoryMap()
        );
        result += TreeSitterRepositoryMapGenerator.GenerateSimpleTreeSitterRepositoryMap(
            _multiplyFile,
            _multiplyRelativeFilePath,
            new RepositoryMap()
        );

        // Assert
        Assert.Equal(expected.TrimEnd(), result.TrimEnd()); // Compare trimmed strings

        return Task.CompletedTask;
    }
}

//     [Fact]
//     public void GetTreeSitterIfAvailable_ShouldReturnProcessedCode_ForCSharpCode()
//     {
//         // Arrange
//         string path = "Add.cs"; // Simulating a C# file path
//
//         // Act
//         var result = TreeSitterParser.GetFullTreeSitterIfAvaialble(_code, path);
//
//         // Assert
//         result.Should().NotBeNull();
//         result.Should().Contain("class.name: Add");
//         result.Should().Contain("base_class.name: IOperation");
//         result.Should().Contain("method.name: AddNumbers");
//         result.Should().Contain("method.name: Calculate");
//         result.Should().Contain("auto_property.name: Result");
//
//         result
//             .Should()
//             .Contain(
//                 @"definition.class: public class Add(double number1, double number2) : IOperation
// {
//     // Property to hold the result of the calculation
//     public double Result { get; private set; }
//
//     public double Calculate()
//     {
//         Result = AddNumbers(); // Assign the result to the property
//         return Result;
//     }
//
//     private double AddNumbers()
//     {
//         return number1 + number2; // Changed to addition from division
//     }
// }"
//             );
//
//         result
//             .Should()
//             .Contain(
//                 @"definition.method: private double AddNumbers()
//     {
//         return number1 + number2; // Changed to addition from division
//     }"
//             );
//
//         result
//             .Should()
//             .Contain(
//                 @"definition.method: private double AddNumbers()
//     {
//         return number1 + number2; // Changed to addition from division
//     }"
//             );
//     }

// [Fact]
// public void GetTreeSitterIfAvailable_ShouldReturnOriginalCode_ForUnsupportedLanguage()
// {
//     // Arrange
//     string path = "file.txt"; // Unsupported file extension
//
//     // Act
//     var result = TreeSitterParser.GetFullTreeSitterIfAvaialble(_code, path);
//
//     // Assert
//     result.Should().Be(_code, "because the language is not supported, so it should return the original code");
// }
//
// [Fact]
// public void GetTreeSitterIfAvailable_ShouldHandleEmptyCodeInput()
// {
//     // Arrange
//     string code = string.Empty;
//     string path = "Add.cs"; // Simulating a C# file path
//
//     // Act
//     var result = TreeSitterParser.GetFullTreeSitterIfAvaialble(code, path);
//
//     // Assert
//     result.Should().BeEmpty("because there is no code to process");
// }
