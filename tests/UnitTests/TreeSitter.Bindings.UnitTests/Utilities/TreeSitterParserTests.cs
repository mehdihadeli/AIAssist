using BuildingBlocks.Types;
using FluentAssertions;
using TreeSitter.Bindings.Utilities;

namespace TreeSitter.Bindings.UnitTests.Utilities;

public class TreeSitterParserTests
{
    private readonly string _code =
        @"

namespace Calculator.Models;
/// <summary>
/// Add two values
/// </summary>
/// <param name=""number1""></param>
/// <param name=""number2""></param>
public class Add(double number1, double number2) : IOperation
{
    // Property to hold the result of the calculation
    public double Result { get; private set; }

    public double Calculate()
    {
        Result = AddNumbers(); // Assign the result to the property
        return Result;
    }
    
    private double AddNumbers()
    {
        return number1 + number2; // Changed to addition from division
    }
}

namespace Calculator;

class Program
{
    static void Main(string[] args)
    {
        // Create an instance of the Add class
        var addition = new Calculator.Models.Add(5.0, 3.0);

        // Calculate the result
        double result = addition.Calculate();

        // Print the result to the console
        Console.WriteLine($""The sum of 5.0 and 3.0 is: {result}"");
    }
}

";

    [Fact]
    public void GetRootNodeExpression_ShouldReturnCorrectExpression_ForCSharpCode()
    {
        // Arrange
        ProgrammingLanguage language = ProgrammingLanguage.Csharp;

        // Act
        var result = TreeSitterParser.GetRootNodeExpression(language, _code);

        // Assert
        result.Should().NotBeNullOrEmpty("because the root node expression should be generated");
    }

    [Fact]
    public void GetTreeSitterIfAvailable_ShouldReturnProcessedCode_ForCSharpCode()
    {
        // Arrange
        string path = "Add.cs"; // Simulating a C# file path

        // Act
        var result = TreeSitterParser.GetTreeSitterIfAvailable(_code, path);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("class.name: Add");
        result.Should().Contain("base_class.name: IOperation");
        result.Should().Contain("method.name: AddNumbers");
        result.Should().Contain("method.name: Calculate");
        result.Should().Contain("auto_property.name: Result");

        result
            .Should()
            .Contain(
                @"definition.class: public class Add(double number1, double number2) : IOperation
{
    // Property to hold the result of the calculation
    public double Result { get; private set; }

    public double Calculate()
    {
        Result = AddNumbers(); // Assign the result to the property
        return Result;
    }
    
    private double AddNumbers()
    {
        return number1 + number2; // Changed to addition from division
    }
}"
            );

        result
            .Should()
            .Contain(
                @"definition.method: private double AddNumbers()
    {
        return number1 + number2; // Changed to addition from division
    }"
            );

        result
            .Should()
            .Contain(
                @"definition.method: private double AddNumbers()
    {
        return number1 + number2; // Changed to addition from division
    }"
            );
    }

    [Fact]
    public void GetParser_ShouldReturnNonNullParser_ForCSharpLanguage()
    {
        // Arrange
        ProgrammingLanguage language = ProgrammingLanguage.Csharp;

        unsafe
        {
            // Act
            var result = TreeSitterParser.GetParser(language);

            var parser = *result;

            // Assert
            parser.Should().NotBeNull("because the C# language is supported, and a valid parser should be returned");
        }
    }

    [Fact]
    public void GetTreeSitterIfAvailable_ShouldReturnOriginalCode_ForUnsupportedLanguage()
    {
        // Arrange
        string path = "file.txt"; // Unsupported file extension

        // Act
        var result = TreeSitterParser.GetTreeSitterIfAvailable(_code, path);

        // Assert
        result.Should().Be(_code, "because the language is not supported, so it should return the original code");
    }

    [Fact]
    public void GetTreeSitterIfAvailable_ShouldHandleEmptyCodeInput()
    {
        // Arrange
        string code = string.Empty;
        string path = "Add.cs"; // Simulating a C# file path

        // Act
        var result = TreeSitterParser.GetTreeSitterIfAvailable(code, path);

        // Assert
        result.Should().BeEmpty("because there is no code to process");
    }

    [Fact]
    public void GetLanguage_ShouldReturnCorrectLanguage_ForCSharpLanguage()
    {
        // Arrange
        ProgrammingLanguage language = ProgrammingLanguage.Csharp;

        unsafe
        {
            // Act
            var result = TreeSitterParser.GetLanguage(language);
            var tsLanguage = *result;

            // Assert
            tsLanguage
                .Should()
                .NotBeNull("because C# is a supported language and should return a valid Tree-sitter language");
        }
    }

    [Fact]
    public void GetLanguage_ShouldReturnNull_ForUnsupportedLanguage()
    {
        // Arrange
        ProgrammingLanguage unsupportedLanguage = (ProgrammingLanguage)999; // Non-existent language enum value

        unsafe
        {
            // Act
            var result = TreeSitterParser.GetLanguage(unsupportedLanguage);
            Assert.True(result == null);
        }
    }
}
