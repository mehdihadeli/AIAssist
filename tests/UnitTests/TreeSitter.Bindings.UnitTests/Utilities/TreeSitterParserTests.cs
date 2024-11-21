using BuildingBlocks.Types;
using FluentAssertions;
using TreeSitter.Bindings.Utilities;

namespace TreeSitter.Bindings.UnitTests.Utilities;

public class TreeSitterParserTests
{
    private TreeSitterParser _treeSitterParser = new();
    private readonly string _code =
        @"

namespace CalculatorApp.Models;
/// <summary>
/// Add two values
/// </summary>
/// <param Name=""number1""></param>
/// <param Name=""number2""></param>
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

namespace CalculatorApp;

class Program
{
    static void Main(string[] args)
    {
        // Create an instance of the Add class
        var addition = new CalculatorApp.Models.Add(5.0, 3.0);

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
        var result = _treeSitterParser.GetRootNodeExpression(language, _code);

        // Assert
        result.Should().NotBeNullOrEmpty("because the root node expression should be generated");
    }

    [Fact]
    public void GetParser_ShouldReturnNonNullParser_ForCSharpLanguage()
    {
        // Arrange
        ProgrammingLanguage language = ProgrammingLanguage.Csharp;

        unsafe
        {
            // Act
            var result = _treeSitterParser.GetParser(language);

            var parser = *result;

            // Assert
            parser.Should().NotBeNull("because the C# language is supported, and a valid parser should be returned");
        }
    }

    [Fact]
    public void GetLanguage_ShouldReturnCorrectLanguage_ForCSharpLanguage()
    {
        // Arrange
        ProgrammingLanguage language = ProgrammingLanguage.Csharp;

        unsafe
        {
            // Act
            var result = _treeSitterParser.GetLanguage(language);
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
            var result = _treeSitterParser.GetLanguage(unsupportedLanguage);
            Assert.True(result == null);
        }
    }
}
