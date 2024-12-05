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
            @"method.

Here's the updated code:

Update: Models/Add.cs"
        );
        AnsiConsole.Write(s);
    }
}
