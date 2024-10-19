using FluentAssertions;
using TreeSitter.Bindings.Utilities;

namespace TreeSitter.Bindings.UnitTests.Utilities;

public class CodeHelperTests
{
    [Fact]
    public void GetLinesOfInterest_ShouldReturnCorrectLines()
    {
        // Arrange
        string codeBlock =
            @"public class MyClass
{
    public int Add(int a, int b)
    {
        return a + b;
    }

    public int Multiply(int a, int b)
    {
        return a * b;
    }
}";

        int[] lineNumbers = [1, 3, 5, 8]; // Specify lines of interest

        // Expected lines based on the input code block and line numbers
        List<string> expectedLines =
        [
            "public class MyClass",
            "    public int Add(int a, int b)",
            "        return a + b;",
            "    public int Multiply(int a, int b)",
        ];

        // Act
        IEnumerable<string?> actualLines = CodeHelper.GetLinesOfInterest(codeBlock, lineNumbers);

        // Assert
        actualLines.Should().BeEquivalentTo(expectedLines);
    }

    [Fact]
    public void GetLinesOfInterest_ShouldHandleEmptyCodeBlock()
    {
        // Arrange
        string codeBlock = ""; // Empty code block
        int[] lineNumbers = [1, 2]; // Line numbers to retrieve

        // Expected output for an empty code block
        List<string> expectedLines = ["", ""];

        // Act
        IEnumerable<string?> actualLines = CodeHelper.GetLinesOfInterest(codeBlock, lineNumbers);

        // Assert
        actualLines.Should().BeEquivalentTo(expectedLines);
    }

    [Fact]
    public void GetLinesOfInterest_ShouldIgnoreOutOfRangeLines()
    {
        // Arrange
        string codeBlock =
            @"public void ExampleMethod()
{
    // Do something
}";

        int[] lineNumbers = [0, 1, 8]; // Includes an out-of-range line number

        // Expected output
        List<string> expectedLines = ["", "public void ExampleMethod()", ""];

        // Act
        IEnumerable<string?> actualLines = CodeHelper.GetLinesOfInterest(codeBlock, lineNumbers);

        // Assert
        actualLines.Should().BeEquivalentTo(expectedLines);
    }

    [Fact]
    public void GetLinesOfInterest_WithEndPattern_ShouldStopAtEndPattern()
    {
        // Arrange
        string codeBlock =
            @"public void Test(
arg1 int,
arg2 int)
{
    // Do something
    Console.WriteLine(""End of method"");
}";

        int[] lineNumbers = [1]; // Starting from line 1
        string endPattern = ")"; // End pattern to search for

        // Expected lines based on the input code block and endPattern
        List<string> expectedLines = ["public void Test(", "arg1 int,", "arg2 int)"];

        // Act
        var actualLines = CodeHelper.GetLinesOfInterest(codeBlock, lineNumbers, [endPattern]);

        // Assert
        actualLines.Should().BeEquivalentTo(expectedLines);
    }

    [Fact]
    public void GetLinesOfInterest_WithEndPattern_NotFound_ShouldReturnLinesUntilEnd()
    {
        // Arrange
        string codeBlock =
            @"public void Test(
arg1 int,
arg2 int)
{
    // Do something
    Console.WriteLine(""End of method"");
}";

        int[] lineNumbers = [1]; // Starting from line 1
        string endPattern = "}"; // End pattern that appears later in the code block

        // Expected lines when endPattern is not immediately found
        List<string> expectedLines =
        [
            "public void Test(",
            "arg1 int,",
            "arg2 int)",
            "{",
            "    // Do something",
            "    Console.WriteLine(\"End of method\");",
            "}",
        ];

        // Act
        var actualLines = CodeHelper.GetLinesOfInterest(codeBlock, lineNumbers, [endPattern]);

        // Assert
        actualLines.Should().BeEquivalentTo(expectedLines);
    }

    [Fact]
    public void GetLinesOfInterest_WithEndPattern_MatchesImmediately_ShouldReturnSingleLine()
    {
        // Arrange
        string codeBlock = @"Console.WriteLine(""Hello, World!"");";

        int[] lineNumbers = [1]; // Starting from line 1
        string endPattern = ";"; // End pattern that matches the line

        // Expected lines when endPattern matches the line itself
        List<string> expectedLines = ["Console.WriteLine(\"Hello, World!\");"];

        // Act
        var actualLines = CodeHelper.GetLinesOfInterest(codeBlock, lineNumbers, [endPattern]);

        // Assert
        actualLines.Should().BeEquivalentTo(expectedLines);
    }

    [Fact]
    public void GetLinesOfInterest_WithEndPattern_NotPresentInCodeBlock_ShouldReturnOnlyTheSpecifiedLine()
    {
        // Arrange
        string codeBlock =
            @"public void Test()
{
    // Do something
}";

        int[] lineNumbers = [1]; // Starting from line 1
        string endPattern = "NonExistentPattern"; // End pattern that does not exist in the code block

        // Expected output: should return only the first line because the end pattern is not found
        List<string> expectedLines = ["public void Test()"];

        // Act
        var actualLines = CodeHelper.GetLinesOfInterest(codeBlock, lineNumbers, [endPattern]);

        // Assert
        actualLines.Should().BeEquivalentTo(expectedLines);
    }

    [Fact]
    public void GetLinesOfInterest_WithEndPatternAndMultipleLineNumbers_ShouldHandleEachLineCorrectly()
    {
        // Arrange
        string codeBlock =
            @"public void Test()
{
    Console.WriteLine(""Hello, World!"");
}

public void AnotherTest()
{
    Console.WriteLine(""Goodbye, World!"");
}";

        int[] lineNumbers = [1, 6]; // Starting from multiple lines
        string endPattern = "}"; // End pattern to look for

        // Expected lines for each specified line number and endPattern
        List<string> expectedLines =
        [
            "public void Test()",
            "{",
            "    Console.WriteLine(\"Hello, World!\");",
            "}",
            "public void AnotherTest()",
            "{",
            "    Console.WriteLine(\"Goodbye, World!\");",
            "}",
        ];

        // Act
        var actualLines = CodeHelper.GetLinesOfInterest(codeBlock, lineNumbers, [endPattern]);

        // Assert
        actualLines.Should().BeEquivalentTo(expectedLines);
    }

    [Fact]
    public void GetLinesOfInterest_ShouldHandleMultipleEndPatterns()
    {
        // Arrange
        string codeBlock =
            @"public void TestMethod(int a,
int b,
string c)
{
    // Some implementation
}";
        int[] lineNumbers = [1]; // Start from line 1
        string[] endPatterns = [")", "{"]; // Multiple end patterns

        // Expected output: should include lines until one ends with ")" or "{"
        List<string> expectedLines = ["public void TestMethod(int a,", "int b,", "string c)"];

        // Act
        IEnumerable<string> actualLines = CodeHelper.GetLinesOfInterest(codeBlock, lineNumbers, endPatterns);

        // Assert
        actualLines.Should().BeEquivalentTo(expectedLines);
    }

    [Fact]
    public void GetChunkOfLines_ValidRange_ReturnsCorrectChunk()
    {
        // Arrange
        string codeBlock =
            @"
using System;
public class Example
{
    public void TestMethod()
    {
        Console.WriteLine(""Hello, World!"");
    }
}";
        int startLine = 2;
        int endLine = 5;

        // Act
        var result = CodeHelper.GetChunkOfLines(codeBlock, startLine, endLine);

        // Assert
        string expected =
            @"using System;
public class Example
{
    public void TestMethod()";
        result.Should().Be(expected);
    }

    [Fact]
    public void GetChunkOfLines_StartLineOutOfRange_ReturnsLinesWithinRange()
    {
        // Arrange
        string codeBlock =
            @"
using System;
public class Example
{
    public void TestMethod()
    {
        Console.WriteLine(""Hello, World!"");
    }
}";
        int startLine = 0; // Out of range
        int endLine = 4;

        // Act
        var result = CodeHelper.GetChunkOfLines(codeBlock, startLine, endLine);

        // Assert
        string expected =
            @"
using System;
public class Example
{";
        result.Should().Be(expected);
    }

    [Fact]
    public void GetLinesOfInterest_SingleLineWithEndPatterns_ShouldReturnCompleteChunk()
    {
        // Arrange
        string codeBlock =
            @"public void ExampleMethod(
int a,
int b)
{
    // Method implementation
}";
        int lineNumber = 1; // Start from line 1
        string[] endPatterns = { ")", "{" }; // Multiple end patterns

        // Expected output: should include lines until one ends with ")" or "{"
        string expectedResult =
            @"public void ExampleMethod(
int a,
int b)";

        // Act
        string actualResult = CodeHelper.GetLinesOfInterest(codeBlock, lineNumber, endPatterns);

        // Assert
        actualResult.Should().Be(expectedResult);
    }

    [Fact]
    public void GetLinesOfInterest_MultipleLineNumbersWithStopPattern_ShouldReturnLinesUpToStopPattern()
    {
        // Arrange
        string codeBlock =
            @"public void MethodA() {
    int a = 1;
    int b = 2; // This line should not be included
}
public void MethodB() {
    // Method B implementation
    int x = 10; // Should stop before this
}
public void MethodC() {}"; // Additional method to ensure we test beyond stop

        int[] lineNumbers = { 1, 2, 3, 4, 5 }; // Requesting lines 1, 2, and 3
        string[] stopPatterns = { "}" }; // Stop pattern that appears in the code block

        // Expected output: should return the first line only because a stop pattern is encountered
        string[] expectedResults =
        {
            "public void MethodA() {",
            "    int a = 1;",
            "    int b = 2; // This line should not be included",
        };

        // Act
        var actualLines = CodeHelper.GetLinesOfInterest(codeBlock, lineNumbers, stopPatterns: stopPatterns);

        // Assert
        actualLines.Should().BeEquivalentTo(expectedResults);
    }

    [Fact]
    public void GetLinesOfInterest_MultipleLinesWithStopPattern_ShouldReturnInitialLine()
    {
        // Arrange
        string codeBlock =
            @"public void MethodA() {
    int a = 1;
    int b = 2;
}
public void MethodB() {}";
        int[] lineNumbers = { 1 };
        string[] stopPatterns = { "}" }; // Stop pattern that appears in the code block

        // Expected output: should return only the first line
        string expectedResult = "public void MethodA() {";

        // Act
        var actualLines = CodeHelper.GetLinesOfInterest(codeBlock, lineNumbers, stopPatterns: stopPatterns);

        // Assert
        actualLines.Should().BeEquivalentTo([expectedResult]);
    }

    [Fact]
    public void GetLinesOfInterest_SingleLineWithNoMatchingEndPatterns_ShouldReturnOnlyTheSpecifiedLine()
    {
        // Arrange
        string codeBlock =
            @"public void AnotherMethod(
int a,
int b)
{
    // Method implementation
}";
        int lineNumber = 1; // Start from line 1
        string[] endPatterns = { ";" }; // End patterns that do not appear in the code block

        // Expected output: should return only the first line because no matching pattern is found
        string expectedResult = "public void AnotherMethod(";

        // Act
        string actualResult = CodeHelper.GetLinesOfInterest(codeBlock, lineNumber, endPatterns, stopPatterns: ["{"]);

        // Assert
        actualResult.Should().Be(expectedResult);
    }

    [Fact]
    public void GetChunkOfLines_EndLineOutOfRange_ReturnsLinesWithinRange()
    {
        // Arrange
        string codeBlock =
            @"
using System;
public class Example
{
    public void TestMethod()
    {
        Console.WriteLine(""Hello, World!"");
    }
}";
        int startLine = 4;
        int endLine = 10; // Out of range

        // Act
        var result = CodeHelper.GetChunkOfLines(codeBlock, startLine, endLine);

        // Assert
        string expected =
            @"{
    public void TestMethod()
    {
        Console.WriteLine(""Hello, World!"");
    }
}";
        result.Should().Be(expected);
    }

    [Fact]
    public void GetChunkOfLines_SingleLineRange_ReturnsSingleLine()
    {
        // Arrange
        string codeBlock =
            @"
using System;
public class Example
{
    public void TestMethod()
    {
        Console.WriteLine(""Hello, World!"");
    }
}";
        int startLine = 5;
        int endLine = 5;

        // Act
        var result = CodeHelper.GetChunkOfLines(codeBlock, startLine, endLine);

        // Assert
        string expected = "    public void TestMethod()";
        result.Should().Be(expected);
    }

    [Fact]
    public void GetChunkOfLines_EmptyCodeBlock_ReturnsEmptyString()
    {
        // Arrange
        string codeBlock = "";
        int startLine = 1;
        int endLine = 3;

        // Act
        var result = CodeHelper.GetChunkOfLines(codeBlock, startLine, endLine);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetChunkOfLines_NegativeRange_ReturnsEmptyString()
    {
        // Arrange
        string codeBlock =
            @"
using System;
public class Example
{
    public void TestMethod()
    {
        Console.WriteLine(""Hello, World!"");
    }
}";
        int startLine = -2;
        int endLine = -1;

        // Act
        var result = CodeHelper.GetChunkOfLines(codeBlock, startLine, endLine);

        // Assert
        result.Should().BeEmpty();
    }
}
