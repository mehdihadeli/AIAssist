using FluentAssertions;
using TreeSitter.Bindings.CustomTypes.TreeParser;
using TreeSitter.Bindings.Utilities;
using static TreeSitter.Bindings.UnitTests.TestData.TestDataConstants;

namespace TreeSitter.Bindings.UnitTests.Utilities;

public class TreeSitterRepositoryMapGeneratorTests : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeSitterRepositoryMap_With_WriteFullTree_False_ShouldGenerateCorrectTreeLevelString_ForAddFile()
    {
        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
            CalculatorApp.AddContentFile,
            CalculatorApp.AddRelativeFilePath,
            new RepositoryMap(),
            writeFullTree: false
        );

        result.Should().Contain("namespace: Calculator");
        result.Should().Contain("class: Add");
        result.Should().Contain("property: public double Result { get; set; }");
        result.Should().Contain("field: public double ResultField");
        result.Should().Contain("method: public double Calculate()");
        result.Should().Contain("method: private double AddNumbers(double first, double second)");

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeSitterRepositoryMap_With_WriteFullTree_True_ShouldGenerateCorrectTreeLevelString_ForAddFile()
    {
        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
            CalculatorApp.AddContentFile,
            CalculatorApp.AddRelativeFilePath,
            new RepositoryMap(),
            writeFullTree: true
        );

        result.Should().Contain("namespace: Calculator");
        result.Should().Contain("class: Add");
        result
            .Should()
            .Contain(
                @"root/
├── Models/
│   ├── Add.cs:
│   │   ├── namespace: Calculator;
│   │   │   ├── class: Add
│   │   │   │   public class Add(double number1, double number2) : IOperation
│   │   │   │   {
│   │   │   │       public double Result { get; set; }
│   │   │   │       public double ResultField;
│   │   │   │       public double ResultField2;
│   │   │   │   
│   │   │   │       /// <summary>
│   │   │   │       /// Calculates the sum of two numbers and updates the result field.
│   │   │   │       /// </summary>
│   │   │   │       /// <returns>The result of the addition as a double.</returns>
│   │   │   │       public double Calculate()
│   │   │   │       {
│   │   │   │           Result = AddNumbers(number1, number2);
│   │   │   │           ResultField = Result;
│   │   │   │   
│   │   │   │           return Result;
│   │   │   │       }
│   │   │   │   
│   │   │   │       private double AddNumbers(double first, double second)
│   │   │   │       {
│   │   │   │           return first + second;
│   │   │   │       }
│   │   │   │   }
"
            );

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeSitterRepositoryMap_With_WriteFullTree_False_ShouldGenerateCorrectTreeLevelString_ForSubtractFile()
    {
        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
            CalculatorApp.SubtractContentFile,
            CalculatorApp.SubtractRelativeFilePath,
            new RepositoryMap(),
            writeFullTree: false
        );

        // Assert for tree structure
        result.Should().Contain("namespace: Calculator");
        result.Should().Contain("class: Subtract");
        result.Should().Contain("field: public double ResultField");
        result.Should().Contain("property: public double Result { get; set; }");
        result.Should().Contain("method: public double Calculate()");

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeSitterRepositoryMap_With_WriteFullTree_True_ShouldGenerateCorrectTreeLevelString_ForSubtractFile()
    {
        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
            CalculatorApp.SubtractContentFile,
            CalculatorApp.SubtractRelativeFilePath,
            new RepositoryMap(),
            writeFullTree: true
        );

        // Assert for tree structure
        result.Should().Contain("namespace: Calculator");
        result.Should().Contain("class: Subtract");
        result
            .Should()
            .Contain(
                @"root/
├── Models/
│   ├── Subtract.cs:
│   │   ├── namespace: Calculator;
│   │   │   ├── class: Subtract
│   │   │   │   public class Subtract(double number1, double number2) : IOperation
│   │   │   │   {
│   │   │   │       public double Result { get; set; }
│   │   │   │       public double ResultField;
│   │   │   │   
│   │   │   │       public double Calculate()
│   │   │   │       {
│   │   │   │           Result = number1 - number2;
│   │   │   │           ResultField = Result;
│   │   │   │   
│   │   │   │           return Result;
│   │   │   │       }
│   │   │   │   }
"
            );

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeSitterRepositoryMap_With_WriteFullTree_False_ShouldGenerateCorrectTreeLevelString_ForDivideFile()
    {
        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
            CalculatorApp.DivideContentFile,
            CalculatorApp.DivideRelativeFilePath,
            new RepositoryMap(),
            writeFullTree: false
        );

        // Assert for tree structure
        result.Should().Contain("namespace: Calculator");
        result.Should().Contain("class: Divide");
        result.Should().Contain("field: public double ResultField");
        result.Should().Contain("property: public double Result { get; set; }");
        result.Should().Contain("method: public double Calculate()");
        result.Should().Contain("method: private double DivideNumbers()");

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeSitterRepositoryMap_With_WriteFullTree_True_ShouldGenerateCorrectTreeLevelString_ForDivideFile()
    {
        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
            CalculatorApp.DivideContentFile,
            CalculatorApp.DivideRelativeFilePath,
            new RepositoryMap(),
            writeFullTree: true
        );

        // Assert for tree structure
        result.Should().Contain("namespace: Calculator");
        result.Should().Contain("class: Divide");
        result
            .Should()
            .Contain(
                @"root/
├── Models/
│   ├── Divide.cs:
│   │   ├── namespace: Calculator;
│   │   │   ├── class: Divide
│   │   │   │   public class Divide(double number1, double number2) : IOperation
│   │   │   │   {
│   │   │   │       public double Result { get; set; }
│   │   │   │       public double ResultField;
│   │   │   │   
│   │   │   │       public double Calculate()
│   │   │   │       {
│   │   │   │           Result = DivideNumbers();
│   │   │   │           ResultField = Result;
│   │   │   │   
│   │   │   │           return Result;
│   │   │   │       }
│   │   │   │   
│   │   │   │       private double DivideNumbers()
│   │   │   │       {
│   │   │   │           if (number1 == 0)
│   │   │   │           {
│   │   │   │               throw new DivideByZeroException(""Cannot divide by zero."");
│   │   │   │           }
│   │   │   │           return number1 / number2;
│   │   │   │       }
│   │   │   │   }
"
            );

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeSitterRepositoryMap_With_WriteFullTree_False_ShouldGenerateCorrectTreeLevelString_ForMultiplyFile()
    {
        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
            CalculatorApp.MultiplyContentFile,
            CalculatorApp.MultiplyRelativeFilePath,
            new RepositoryMap(),
            writeFullTree: false
        );

        // Assert for tree structure
        result.Should().Contain("namespace: Calculator");
        result.Should().Contain("class: Multiply");
        result.Should().Contain("field: public double ResultField");
        result.Should().Contain("property: public double Result { get; set; }");
        result.Should().Contain("method: public double Calculate()");

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeSitterRepositoryMap_With_WriteFullTree_True_ShouldGenerateCorrectTreeLevelString_ForMultiplyFile()
    {
        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
            CalculatorApp.MultiplyContentFile,
            CalculatorApp.MultiplyRelativeFilePath,
            new RepositoryMap(),
            writeFullTree: true
        );

        // Assert for tree structure
        result.Should().Contain("namespace: Calculator");
        result.Should().Contain("class: Multiply");
        result
            .Should()
            .Contain(
                @"root/
├── Models/
│   ├── Multiply.cs:
│   │   ├── namespace: Calculator;
│   │   │   ├── class: Multiply
│   │   │   │   public class Multiply(double number1, double number2) : IOperation
│   │   │   │   {
│   │   │   │       public double Result { get; set; }
│   │   │   │       public double ResultField;
│   │   │   │   
│   │   │   │       public double Calculate()
│   │   │   │       {
│   │   │   │           Result = number1 * number2;
│   │   │   │           ResultField = Result;
│   │   │   │   
│   │   │   │           return Result;
│   │   │   │       }
│   │   │   │   }
"
            );

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeSitterRepositoryMap_With_WriteFullTree_False_ShouldGenerateCorrectTreeLevelString_ForProgramFile()
    {
        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
            CalculatorApp.ProgramContentFile,
            CalculatorApp.ProgramRelativeFilePath,
            new RepositoryMap(),
            writeFullTree: false
        );

        // Assert for tree structure
        result.Should().Contain("Program.cs");
        result.Should().Contain("Top-level statement: Console.WriteLine(\"Simple Calculator\\n\");");
        result
            .Should()
            .Contain(
                @"root/
├── Program.cs:
│   ├── Top-level statement: Console.WriteLine(""Simple Calculator\n"");
"
            );

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeSitterRepositoryMap_With_WriteFullTree_True_ShouldGenerateCorrectTreeLevelString_ForProgramFile()
    {
        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
            CalculatorApp.ProgramContentFile,
            CalculatorApp.ProgramRelativeFilePath,
            new RepositoryMap(),
            writeFullTree: true
        );

        // Assert for tree structure
        result.Should().Contain("Program.cs");
        result.Should().Contain("Top-level statement:");
        result
            .Should()
            .Contain(
                @"root/
├── Program.cs:
│   ├── Top-level statement: 
│   │   Console.WriteLine(""Simple Calculator\n"");
│   │   Console.Write(""Enter the first number: "");
│   │   double num1 = Convert.ToDouble(Console.ReadLine());
│   │   Console.Write(""Enter an operation (+, -, *, /): "");
│   │   char operation = Convert.ToChar(Console.ReadLine());
│   │   Console.Write(""Enter the second number: "");
│   │   double num2 = Convert.ToDouble(Console.ReadLine());
│   │   IOperation calculation = operation switch
│   │   {
│   │       '+' => new Add(num1, num2),
│   │       '-' => new Subtract(num1, num2),
│   │       '*' => new Multiply(num1, num2),
│   │       '/' => new Divide(num1, num2),
│   │       _ => throw new InvalidOperationException(""Invalid operation""),
│   │   };
│   │   double result = calculation.Calculate();
│   │   Console.WriteLine($""Result: {result}"");
"
            );

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeSitterRepositoryMap_With_WriteFullTree_False_ShouldReturnEmpty_ForEmptyFile()
    {
        // Arrange
        string emptyFileContent = string.Empty;

        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
            emptyFileContent,
            CalculatorApp.SubtractRelativeFilePath,
            new RepositoryMap(),
            writeFullTree: false
        );

        // Assert
        result
            .Should()
            .Be(
                @"root/
├── Models/
│   ├── Subtract.cs:
"
            );

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeSitterRepositoryMap_With_WriteFullTree_False_ShouldMatchExpectedOutput_ForProjectStructure()
    {
        // Arrange
        var expectedProgram =
            @"root/
├── Program.cs:
│   ├── Top-level statement: Console.WriteLine(""Simple Calculator\n"");
";

        var expectedAdd =
            @"root/
├── Models/
│   ├── Add.cs:
│   │   ├── namespace: Calculator
│   │   │   ├── class: Add
│   │   │   │   ├── property: public double Result { get; set; }
│   │   │   │   ├── method: public double Calculate();
│   │   │   │   ├── method: private double AddNumbers(double first, double second);
│   │   │   │   ├── field: public double ResultField;
│   │   │   │   ├── field: public double ResultField2;
";

        var expectedSubtract =
            @"root/
├── Models/
│   ├── Subtract.cs:
│   │   ├── namespace: Calculator
│   │   │   ├── class: Subtract
│   │   │   │   ├── property: public double Result { get; set; }
│   │   │   │   ├── method: public double Calculate();
│   │   │   │   ├── field: public double ResultField;
";

        var expectedDivide =
            @"root/
├── Models/
│   ├── Divide.cs:
│   │   ├── namespace: Calculator
│   │   │   ├── class: Divide
│   │   │   │   ├── property: public double Result { get; set; }
│   │   │   │   ├── method: public double Calculate();
│   │   │   │   ├── method: private double DivideNumbers();
│   │   │   │   ├── field: public double ResultField;
";

        var expectedMultiply =
            @"root/
├── Models/
│   ├── Multiply.cs:
│   │   ├── namespace: Calculator
│   │   │   ├── class: Multiply
│   │   │   │   ├── property: public double Result { get; set; }
│   │   │   │   ├── method: public double Calculate();
│   │   │   │   ├── field: public double ResultField;
";
        // Act
        var programResult = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
            CalculatorApp.ProgramContentFile,
            CalculatorApp.ProgramRelativeFilePath,
            new RepositoryMap(),
            writeFullTree: false
        );
        // Assert
        programResult.TrimEnd().Should().Be(expectedProgram.TrimEnd());

        var addResult = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
            CalculatorApp.AddContentFile,
            CalculatorApp.AddRelativeFilePath,
            new RepositoryMap(),
            writeFullTree: false
        );
        // Assert
        addResult.TrimEnd().Should().Be(expectedAdd.TrimEnd());

        var subtractResult = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
            CalculatorApp.SubtractContentFile,
            CalculatorApp.SubtractRelativeFilePath,
            new RepositoryMap(),
            writeFullTree: false
        );
        // Assert
        subtractResult.TrimEnd().Should().Be(expectedSubtract.TrimEnd());

        var divideResult = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
            CalculatorApp.DivideContentFile,
            CalculatorApp.DivideRelativeFilePath,
            new RepositoryMap(),
            writeFullTree: false
        );
        // Assert
        divideResult.TrimEnd().Should().Be(expectedDivide.TrimEnd());

        var multiplyResult = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
            CalculatorApp.MultiplyContentFile,
            CalculatorApp.MultiplyRelativeFilePath,
            new RepositoryMap(),
            writeFullTree: false
        );
        // Assert
        multiplyResult.TrimEnd().Should().Be(expectedMultiply.TrimEnd());

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeSitterRepositoryMap_With_WriteFullTree_False_ShouldGenerateCorrectTreeLevelString_ForSimpleCalculator()
    {
        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
            SimpleCalculatorApp.ProgramContentFile,
            SimpleCalculatorApp.ProgramRelativeFilePath,
            new RepositoryMap(),
            writeFullTree: false
        );

        result.Should().Contain("namespace: SimpleCalculator");

        // Assert class `Calculator` with its methods
        result.Should().Contain("class: Calculator");
        result.Should().Contain("method: public double Calculate(double a, double b, Operation operation);");
        result.Should().Contain("method: public CalculationResult PerformCalculation(Calculation calc);");

        // Assert class `Program` with its method
        result.Should().Contain("class: Program");
        result.Should().Contain("method: static void Main(string[] args);");

        // Assert enum `Operation`
        result.Should().Contain("enum: public enum Operation { Add, Subtract, Multiply, Divide, }");

        // Assert record `CalculationResult`
        result.Should().Contain("record: CalculationResult");

        // Assert interface `ICalculator`
        result.Should().Contain("interface: ICalculator");

        // Assert struct `Calculation` with properties
        result.Should().Contain("struct: Calculation");
        result.Should().Contain("property: public double Operand1 { get; }");
        result.Should().Contain("property: public double Operand2 { get; }");

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeSitterRepositoryMap_With_WriteFullTree_True_ShouldGenerateCorrectTreeLevelString_ForSimpleCalculator()
    {
        // Act
        var result = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
            SimpleCalculatorApp.ProgramContentFile,
            SimpleCalculatorApp.ProgramRelativeFilePath,
            new RepositoryMap(),
            writeFullTree: true
        );

        result.Should().Contain("namespace: SimpleCalculator");

        // Assert that the result contains the Calculator class and its methods
        result.Should().Contain("class: Calculator");
        result.Should().Contain("public double Calculate(double a, double b, Operation operation)");
        result.Should().Contain("public CalculationResult PerformCalculation(Calculation calc)");

        // Assert that the result contains the Program class and its Main method
        result.Should().Contain("class: Program");
        result.Should().Contain("static void Main(string[] args)");

        // Assert that the result contains the Operation enum and its members
        result.Should().Contain("enum: Operation");
        result.Should().Contain("Add");
        result.Should().Contain("Subtract");
        result.Should().Contain("Multiply");
        result.Should().Contain("Divide");

        // Assert that the result contains the CalculationResult record
        result.Should().Contain("record: CalculationResult");
        result.Should().Contain("public record CalculationResult(double Result, string Description)");

        // Assert that the result contains the ICalculator interface and its method
        result.Should().Contain("interface: ICalculator");
        result.Should().Contain("double Calculate(double a, double b, Operation operation)");

        // Assert that the result contains the Calculation struct and its fields
        result.Should().Contain("struct: Calculation");
        result.Should().Contain("public double Operand1 { get; }");
        result.Should().Contain("public double Operand2 { get; }");
        result.Should().Contain("public Operation Operation { get; }");

        result
            .Should()
            .Contain(
                @"│   │   ├── class: Calculator
│   │   │   public class Calculator : ICalculator
│   │   │       {
│   │   │           /// <summary>
│   │   │           /// Calculate the operation
│   │   │           /// </summary>
│   │   │           /// <param name=""a""></param>
│   │   │           /// <param name=""b""></param>
│   │   │           /// <param name=""operation""></param>
│   │   │           /// <returns></returns>
│   │   │           /// <exception cref=""DivideByZeroException""></exception>
│   │   │           /// <exception cref=""ArgumentOutOfRangeException""></exception>
│   │   │           public double Calculate(double a, double b, Operation operation)
│   │   │           {
│   │   │               return operation switch
│   │   │               {
│   │   │                   Operation.Add => a + b,
│   │   │                   Operation.Subtract => a - b,
│   │   │                   Operation.Multiply => a * b,
│   │   │                   Operation.Divide => b != 0 ? a / b : throw new DivideByZeroException(""Cannot divide by zero.""),
│   │   │                   _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null),
│   │   │               };
│   │   │           }
│   │   │   
│   │   │           public CalculationResult PerformCalculation(Calculation calc)
│   │   │           {
│   │   │               double result = Calculate(calc.Operand1, calc.Operand2, calc.Operation);
│   │   │               string description = $""{calc.Operand1} {calc.Operation} {calc.Operand2} = {result}"";
│   │   │               return new CalculationResult(result, description);
│   │   │           }
│   │   │       }"
            );

        result
            .Should()
            .Contain(
                @"│   │   ├── class: Program
│   │   │   class Program
│   │   │       {
│   │   │           static void Main(string[] args)
│   │   │           {
│   │   │               var calculator = new Calculator();
│   │   │   
│   │   │               Console.WriteLine(""Simple Calculator"");
│   │   │               Console.WriteLine(""Choose an operation: Add, Subtract, Multiply, Divide"");
│   │   │               string userInput = Console.ReadLine();
│   │   │               Operation operation;
│   │   │   
│   │   │               if (Enum.TryParse(userInput, true, out operation))
│   │   │               {
│   │   │                   Console.Write(""Enter first number: "");
│   │   │                   double operand1 = Convert.ToDouble(Console.ReadLine());
│   │   │   
│   │   │                   Console.Write(""Enter second number: "");
│   │   │                   double operand2 = Convert.ToDouble(Console.ReadLine());
│   │   │   
│   │   │                   Calculation calculation = new Calculation(operand1, operand2, operation);
│   │   │                   CalculationResult result = calculator.PerformCalculation(calculation);
│   │   │   
│   │   │                   Console.WriteLine(result.Description);
│   │   │               }
│   │   │               else
│   │   │               {
│   │   │                   Console.WriteLine(""Invalid operation."");
│   │   │               }
│   │   │           }
│   │   │       }"
            );

        result
            .Should()
            .Contain(
                @"│   │   ├── interface: ICalculator
│   │   │   public interface ICalculator
│   │   │       {
│   │   │           double Calculate(double a, double b, Operation operation);
│   │   │       }"
            );

        return Task.CompletedTask;
    }

    [Fact]
    public Task GenerateTreeSitterRepositoryMap_With_WriteFullTree_True_ShouldMatchExpectedOutput_ForSimpleCalculatorProjectStructure()
    {
        var result = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
            SimpleCalculatorApp.ProgramContentFile,
            SimpleCalculatorApp.ProgramRelativeFilePath,
            new RepositoryMap(),
            writeFullTree: true
        );

        result
            .Should()
            .Contain(
                @"root/
├── Program.cs:
│   ├── namespace: SimpleCalculator;
│   │   ├── class: Calculator
│   │   │   public class Calculator : ICalculator
│   │   │       {
│   │   │           /// <summary>
│   │   │           /// Calculate the operation
│   │   │           /// </summary>
│   │   │           /// <param name=""a""></param>
│   │   │           /// <param name=""b""></param>
│   │   │           /// <param name=""operation""></param>
│   │   │           /// <returns></returns>
│   │   │           /// <exception cref=""DivideByZeroException""></exception>
│   │   │           /// <exception cref=""ArgumentOutOfRangeException""></exception>
│   │   │           public double Calculate(double a, double b, Operation operation)
│   │   │           {
│   │   │               return operation switch
│   │   │               {
│   │   │                   Operation.Add => a + b,
│   │   │                   Operation.Subtract => a - b,
│   │   │                   Operation.Multiply => a * b,
│   │   │                   Operation.Divide => b != 0 ? a / b : throw new DivideByZeroException(""Cannot divide by zero.""),
│   │   │                   _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null),
│   │   │               };
│   │   │           }
│   │   │   
│   │   │           public CalculationResult PerformCalculation(Calculation calc)
│   │   │           {
│   │   │               double result = Calculate(calc.Operand1, calc.Operand2, calc.Operation);
│   │   │               string description = $""{calc.Operand1} {calc.Operation} {calc.Operand2} = {result}"";
│   │   │               return new CalculationResult(result, description);
│   │   │           }
│   │   │       }
│   │   ├── class: Program
│   │   │   class Program
│   │   │       {
│   │   │           static void Main(string[] args)
│   │   │           {
│   │   │               var calculator = new Calculator();
│   │   │   
│   │   │               Console.WriteLine(""Simple Calculator"");
│   │   │               Console.WriteLine(""Choose an operation: Add, Subtract, Multiply, Divide"");
│   │   │               string userInput = Console.ReadLine();
│   │   │               Operation operation;
│   │   │   
│   │   │               if (Enum.TryParse(userInput, true, out operation))
│   │   │               {
│   │   │                   Console.Write(""Enter first number: "");
│   │   │                   double operand1 = Convert.ToDouble(Console.ReadLine());
│   │   │   
│   │   │                   Console.Write(""Enter second number: "");
│   │   │                   double operand2 = Convert.ToDouble(Console.ReadLine());
│   │   │   
│   │   │                   Calculation calculation = new Calculation(operand1, operand2, operation);
│   │   │                   CalculationResult result = calculator.PerformCalculation(calculation);
│   │   │   
│   │   │                   Console.WriteLine(result.Description);
│   │   │               }
│   │   │               else
│   │   │               {
│   │   │                   Console.WriteLine(""Invalid operation."");
│   │   │               }
│   │   │           }
│   │   │       }
│   │   ├── enum: Operation
│   │   │   public enum Operation
│   │   │       {
│   │   │           Add,
│   │   │           Subtract,
│   │   │           Multiply,
│   │   │           Divide,
│   │   │       }
│   │   ├── record: CalculationResult
│   │   │   public record CalculationResult(double Result, string Description);
│   │   ├── interface: ICalculator
│   │   │   public interface ICalculator
│   │   │       {
│   │   │           double Calculate(double a, double b, Operation operation);
│   │   │       }
│   │   ├── struct: Calculation
│   │   │   public struct Calculation
│   │   │       {
│   │   │           public double Operand1 { get; }
│   │   │           public double Operand2 { get; }
│   │   │           public Operation Operation { get; }
│   │   │   
│   │   │           public Calculation(double operand1, double operand2, Operation operation)
│   │   │           {
│   │   │               Operand1 = operand1;
│   │   │               Operand2 = operand2;
│   │   │               Operation = operation;
│   │   │           }
│   │   │       }
"
            );

        return Task.CompletedTask;
    }
}
