using BuildingBlocks.SpectreConsole.Markdown;
using Spectre.Console;
using Xunit;

namespace BuildingBlocks.UnitTests.SpectreConsole.Markdown;

public class SpectreMarkdownTests
{
    [Fact]
    public void Test()
    {
        var s = new SpectreMarkdown(
            @"1. **Calculate()**: 
   - **Description**: Calculates the sum of two numbers and updates the result field.
   - **Returns**: The result of the addition as a double.
2. **AddNumbers(double first, double second)**: 
   - **Description**: A private method that adds two numbers.
   - **Returns**: The sum of the two numbers."
        );
        AnsiConsole.Write(s);
    }

    [Fact]
    public void Test2()
    {
        var s = new SpectreMarkdown(
            @"Here are the method names inside the `Add` class:

- `Calculate()`
- `AddNumbers(double first, double second)`"
        );
        AnsiConsole.Write(s);
    }

    [Fact]
    public void Test3()
    {
        var s = new SpectreMarkdown(
            @"To add a method overload to the `Add` class, we need to modify the `Add` class in the `Models/Add.cs` file. Since the current context does not provide the full implementation of the `Add` class, I will assume a basic structure and add an overloaded method.

Here's the updated code:

Update: Models/Add.cs
```csharp
namespace Calculator;"
        );
        AnsiConsole.Write(s);
    }
}
