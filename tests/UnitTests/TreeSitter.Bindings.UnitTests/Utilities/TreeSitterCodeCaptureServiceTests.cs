using FluentAssertions;
using TreeSitter.Bindings.CustomTypes.TreeParser;
using TreeSitter.Bindings.Services;
using TreeSitter.Bindings.UnitTests.TestData;
using TreeSitter.Bindings.Utilities;

namespace TreeSitter.Bindings.UnitTests.Utilities;

public class TreeSitterCodeCaptureServiceTests
{
    private readonly TreeSitterCodeCaptureService _treeSitterCodeCaptureService = new TreeSitterCodeCaptureService(
        new TreeSitterParser()
    );

    [Fact]
    public void CreateTreeSitterMap_WithValidCSharpFiles_ShouldReturnCodeFileMaps()
    {
        // Arrange
        var codeFiles = new List<CodeFile>
        {
            new(
                Code: TestDataConstants.CalculatorApp.ProgramContentFile,
                RelativePath: TestDataConstants.CalculatorApp.ProgramRelativeFilePath
            ),
            new(
                Code: TestDataConstants.CalculatorApp.AddContentFile,
                RelativePath: TestDataConstants.CalculatorApp.AddRelativeFilePath
            ),
        };

        // Act
        var result = _treeSitterCodeCaptureService.CreateTreeSitterMap(codeFiles);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var programMap = result.FirstOrDefault(x =>
            x.RelativePath == TestDataConstants.CalculatorApp.ProgramRelativeFilePath
        );
        programMap.Should().NotBeNull();
        // programMap.TreeSitterFullCode.Should().NotBeNullOrEmpty();
        programMap.OriginalCode.Should().NotBeNullOrEmpty();
        programMap.OriginalCode.Should().Be(TestDataConstants.CalculatorApp.ProgramContentFile);
        // programMap.TreeSitterSummarizeCode.Should().NotBeNullOrEmpty();

        // Assert the structure of the generated tree for Program.cs
        //         programMap.TreeSitterFullCode.Should().Contain("├── name.top_level_statement:");
        //         programMap
        //             .TreeSitterFullCode.Should()
        //             .Contain(
        //                 @"Program.cs:
        // ⋮...
        // ├── name.top_level_statement:
        // │   ├── Console.WriteLine(""Simple Calculator\n"");
        // │   │
        // │   │   // Input first number
        // │   │   Console.Write(""Enter the first number: "");
        // │   │   double num1 = Convert.ToDouble(Console.ReadLine());
        // │   │
        // │   │   // Input operator
        // │   │   Console.Write(""Enter an operation (+, -, *, /): "");
        // │   │   char operation = Convert.ToChar(Console.ReadLine());
        // │   │
        // │   │   // Input second number
        // │   │   Console.Write(""Enter the second number: "");
        // │   │   double num2 = Convert.ToDouble(Console.ReadLine());
        // │   │
        // │   │   IOperation calculation = operation switch
        // │   │   {
        // │   │   '+' => new Add(num1, num2),
        // │   │   '-' => new Subtract(num1, num2),
        // │   │   '*' => new Multiply(num1, num2),
        // │   │   '/' => new Divide(num1, num2),
        // │   │   _ => throw new InvalidOperationException(""Invalid operation""),
        // │   │   };
        // │   │
        // │   │   double result = calculation.Calculate();
        // │   │   Console.WriteLine($""Result: {result}"");
        // │   │
        // │   ⋮...
        // "
        //             );
        //
        //         programMap
        //             .TreeSitterSummarizeCode.Should()
        //             .Contain(
        //                 @"Program.cs:
        // ⋮...
        // ├── name.top_level_statement:
        // │   ├── Console.WriteLine(""Simple Calculator\n"");
        // │   ⋮...
        // "
        //             );

        var addMap = result.FirstOrDefault(x => x.RelativePath == TestDataConstants.CalculatorApp.AddRelativeFilePath);
        addMap.Should().NotBeNull();
        // addMap.TreeSitterFullCode.Should().NotBeNullOrEmpty();
        addMap.OriginalCode.Should().NotBeNullOrEmpty();
        addMap.OriginalCode.Should().Be(TestDataConstants.CalculatorApp.AddContentFile);
        //         addMap.TreeSitterSummarizeCode.Should().NotBeNullOrEmpty();
        //
        //         // Assert the structure of the generated tree for Add.cs
        //         addMap
        //             .TreeSitterFullCode.Should()
        //             .Contain(
        //                 @"Models/Add.cs:
        // ⋮...
        // ├── name.class:
        // │   ├── public class Add(double number1, double number2) : IOperation
        // │   │   {
        // │   │   public double Result { get; set; }
        // │   │   public double ResultField;
        // │   │   public double ResultField2;
        // │   │
        // │   │   /// <summary>
        // │   │   /// Calculates the sum of two numbers and updates the result field.
        // │   │   /// </summary>
        // │   │   /// <returns>The result of the addition as a double.</returns>
        // │   │   public double Calculate()
        // │   │   {
        // │   │   Result = AddNumbers(number1, number2);
        // │   │   ResultField = Result;
        // │   │
        // │   │   return Result;
        // │   │   }
        // │   │
        // │   │   private double AddNumbers(double first, double second)
        // │   │   {
        // │   │   return first + second;
        // │   │   }
        // │   │   }
        // │   │
        // │   ⋮...
        // ├── name.field:
        // │   ├── public double ResultField;
        // │   │   public double ResultField2;
        // │   │
        // │   ⋮...
        // ├── name.file_scoped_namespace:
        // │   ├── namespace Calculator;
        // │   │
        // │   ⋮...
        // ├── name.method:
        // │   ├── public double Calculate()
        // │   │   {
        // │   │   Result = AddNumbers(number1, number2);
        // │   │   ResultField = Result;
        // │   │
        // │   │   return Result;
        // │   │   }
        // │   │   private double AddNumbers(double first, double second)
        // │   │   {
        // │   │   return first + second;
        // │   │   }
        // │   │
        // │   ⋮...
        // ├── name.property:
        // │   ├── public double Result { get; set; }
        // │   │
        // │   ⋮...
        // "
        //             );
        //
        //         addMap
        //             .TreeSitterSummarizeCode.Should()
        //             .Contain(
        //                 @"Models/Add.cs:
        // ⋮...
        // ├── name.class:
        // │   ├── public class Add(double number1, double number2) : IOperation
        // │   ⋮...
        // ├── name.field:
        // │   ├── public double ResultField;
        // │   ├── public double ResultField2;
        // │   ⋮...
        // ├── name.file_scoped_namespace:
        // │   ├── namespace Calculator;
        // │   ⋮...
        // ├── name.method:
        // │   ├── private double AddNumbers(double first, double second)
        // │   ├── public double Calculate()
        // │   ⋮...
        // ├── name.property:
        // │   ├── public double Result { get; set; }
        // │   ⋮...
        // "
        //             );
    }

    [Fact]
    public void CreateTreeSitterMap_WithEmptyCodeFiles_ShouldReturnEmptyResult()
    {
        // Act
        var result = _treeSitterCodeCaptureService.CreateTreeSitterMap(new List<CodeFile>());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void CreateTreeSitterMap_WithInvalidLanguageFile_ShouldReturnCaptureItemWithOriginalCode()
    {
        // Arrange
        var codeFiles = new List<CodeFile>
        {
            new(Code: "Some content that is not code", RelativePath: "InvalidFile.txt"),
        };

        // Act
        var result = _treeSitterCodeCaptureService.CreateTreeSitterMap(codeFiles);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);

        var invalidFileMap = result.FirstOrDefault(x => x.RelativePath == "InvalidFile.txt");
        invalidFileMap.Should().NotBeNull();
        invalidFileMap.OriginalCode.Should().Be("Some content that is not code");
        //         invalidFileMap.TreeSitterFullCode.Should().NotBeNullOrEmpty();
        //         invalidFileMap
        //             .TreeSitterFullCode.Should()
        //             .Be(
        //                 @"InvalidFile.txt:
        // ⋮...
        // ├── code definition:
        // │   ├── Some content that is not code
        // │   ⋮...
        // "
        //             );
        //         invalidFileMap.TreeSitterSummarizeCode.Should().NotBeNullOrEmpty();
        //         invalidFileMap
        //             .TreeSitterSummarizeCode.Should()
        //             .Be(
        //                 @"InvalidFile.txt:
        // ⋮...
        // ├── code definition:
        // │   ├── Some content that is not code
        // │   ⋮...
        // "
        //             );

        var captureItem = invalidFileMap.DefinitionCaptureReferences.FirstOrDefault();
        captureItem.Should().BeNull();
    }

    [Fact]
    public void CreateTreeSitterMap_WithFileHavingNoCaptures_ShouldReturnInputCode()
    {
        // Arrange
        var codeFiles = new List<CodeFile>
        {
            new(Code: "NoCaptureElement {}", RelativePath: TestDataConstants.CalculatorApp.SubtractRelativeFilePath),
        };

        // Act
        var result = _treeSitterCodeCaptureService.CreateTreeSitterMap(codeFiles);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        var subtractMap = result.First();
        // subtractMap.TreeSitterFullCode.Should().NotBeEmpty().And.Contain("NoCaptureElement {}");
        // subtractMap.TreeSitterSummarizeCode.Should().NotBeEmpty().And.Contain("NoCaptureElement {}");
    }

    [Fact]
    public void CreateTreeSitterMap_WithMultipleFiles_ShouldHandleCorrectly()
    {
        // Arrange
        var codeFiles = new List<CodeFile>
        {
            new(
                Code: TestDataConstants.CalculatorApp.ProgramContentFile,
                RelativePath: TestDataConstants.CalculatorApp.ProgramRelativeFilePath
            ),
            new(
                Code: TestDataConstants.CalculatorApp.AddContentFile,
                TestDataConstants.CalculatorApp.AddRelativeFilePath
            ),
            new(
                Code: TestDataConstants.CalculatorApp.SubtractContentFile,
                RelativePath: TestDataConstants.CalculatorApp.SubtractRelativeFilePath
            ),
        };

        // Act
        var result = _treeSitterCodeCaptureService.CreateTreeSitterMap(codeFiles);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);

        var programMap = result.FirstOrDefault(x =>
            x.RelativePath == TestDataConstants.CalculatorApp.ProgramRelativeFilePath
        );
        programMap.Should().NotBeNull();

        var addMap = result.FirstOrDefault(x => x.RelativePath == TestDataConstants.CalculatorApp.AddRelativeFilePath);
        addMap.Should().NotBeNull();

        var subtractMap = result.FirstOrDefault(x =>
            x.RelativePath == TestDataConstants.CalculatorApp.SubtractRelativeFilePath
        );
        subtractMap.Should().NotBeNull();
    }

    [Fact]
    public void CreateTreeSitterMap_WithFullCalculatorAppFiles_ShouldReturnExpectedMappings()
    {
        // Arrange
        var codeFiles = new List<CodeFile>
        {
            new(
                Code: TestDataConstants.CalculatorApp.ProgramContentFile,
                RelativePath: TestDataConstants.CalculatorApp.ProgramRelativeFilePath
            ),
            new(
                Code: TestDataConstants.CalculatorApp.OperationContentFile,
                RelativePath: TestDataConstants.CalculatorApp.OperationRelativeFilePath
            ),
            new(
                Code: TestDataConstants.CalculatorApp.AddContentFile,
                RelativePath: TestDataConstants.CalculatorApp.AddRelativeFilePath
            ),
            new(
                Code: TestDataConstants.CalculatorApp.SubtractContentFile,
                RelativePath: TestDataConstants.CalculatorApp.SubtractRelativeFilePath
            ),
            new(
                Code: TestDataConstants.CalculatorApp.MultiplyContentFile,
                RelativePath: TestDataConstants.CalculatorApp.MultiplyRelativeFilePath
            ),
            new(
                Code: TestDataConstants.CalculatorApp.DivideContentFile,
                RelativePath: TestDataConstants.CalculatorApp.DivideRelativeFilePath
            ),
        };

        // Act
        var result = _treeSitterCodeCaptureService.CreateTreeSitterMap(codeFiles);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(6);

        // Verify mappings for each file
        var programMap = result.FirstOrDefault(x =>
            x.RelativePath == TestDataConstants.CalculatorApp.ProgramRelativeFilePath
        );
        programMap.Should().NotBeNull();
        // programMap.TreeSitterFullCode.Should().NotBeNullOrEmpty();
        // programMap.TreeSitterSummarizeCode.Should().NotBeNullOrEmpty();
        programMap.OriginalCode.Should().Be(TestDataConstants.CalculatorApp.ProgramContentFile);

        var operationMap = result.FirstOrDefault(x =>
            x.RelativePath == TestDataConstants.CalculatorApp.OperationRelativeFilePath
        );
        operationMap.Should().NotBeNull();
        // operationMap.TreeSitterFullCode.Should().NotBeNullOrEmpty();
        // operationMap.TreeSitterSummarizeCode.Should().NotBeNullOrEmpty();
        operationMap.OriginalCode.Should().Be(TestDataConstants.CalculatorApp.OperationContentFile);

        var addMap = result.FirstOrDefault(x => x.RelativePath == TestDataConstants.CalculatorApp.AddRelativeFilePath);
        addMap.Should().NotBeNull();
        // addMap.TreeSitterFullCode.Should().NotBeNullOrEmpty();
        // addMap.TreeSitterSummarizeCode.Should().NotBeNullOrEmpty();
        addMap.OriginalCode.Should().Be(TestDataConstants.CalculatorApp.AddContentFile);

        var subtractMap = result.FirstOrDefault(x =>
            x.RelativePath == TestDataConstants.CalculatorApp.SubtractRelativeFilePath
        );
        subtractMap.Should().NotBeNull();
        // subtractMap.TreeSitterFullCode.Should().NotBeNullOrEmpty();
        // subtractMap.TreeSitterSummarizeCode.Should().NotBeNullOrEmpty();
        subtractMap.OriginalCode.Should().Be(TestDataConstants.CalculatorApp.SubtractContentFile);

        var multiplyMap = result.FirstOrDefault(x =>
            x.RelativePath == TestDataConstants.CalculatorApp.MultiplyRelativeFilePath
        );
        multiplyMap.Should().NotBeNull();
        // multiplyMap.TreeSitterFullCode.Should().NotBeNullOrEmpty();
        // multiplyMap.TreeSitterSummarizeCode.Should().NotBeNullOrEmpty();
        multiplyMap.OriginalCode.Should().Be(TestDataConstants.CalculatorApp.MultiplyContentFile);

        var divideMap = result.FirstOrDefault(x =>
            x.RelativePath == TestDataConstants.CalculatorApp.DivideRelativeFilePath
        );
        divideMap.Should().NotBeNull();
        // divideMap.TreeSitterFullCode.Should().NotBeNullOrEmpty();
        // divideMap.TreeSitterSummarizeCode.Should().NotBeNullOrEmpty();
        divideMap.OriginalCode.Should().Be(TestDataConstants.CalculatorApp.DivideContentFile);

        // Additional checks for reference and definition linking
        var operationFileMap = result.SingleOrDefault(x => x.RelativePath == "IOperation.cs");
        operationFileMap.Should().NotBeNull();
        // operationFileMap.ReferencedCodesMap.Should().NotBeNull();
        // operationFileMap.ReferencedCodesMap.Should().NotBeEmpty();
    }
}
