using AIAssistant.Services;
using FluentAssertions;

namespace AIAssistant.IntegrationTests.Services;

public class CodeLoaderServiceTests : IAsyncLifetime
{
    private string _appWorkingDir = default!;
    private string _originalWorkingDir = default!;

    public async Task InitializeAsync()
    {
        _originalWorkingDir = Directory.GetCurrentDirectory();

        // Save the original working directory
        _appWorkingDir = Path.Combine(Directory.GetCurrentDirectory(), "TestData/Calculator");

        // Change the working directory to the new test directory
        Directory.SetCurrentDirectory(_appWorkingDir);

        await Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Directory.SetCurrentDirectory(_originalWorkingDir);
        return Task.CompletedTask;
    }

    [Fact]
    public void LoadApplicationCodes_ShouldLoadAllValidFiles()
    {
        // Arrange
        var service = new CodeLoaderService();

        // Act
        var result = service.LoadApplicationCodes(_appWorkingDir).ToList();

        // Assert
        result.Should().HaveCount(8);
        result.Should().ContainSingle(code => code.RelativePath == "Program.cs");
        result.Should().ContainSingle(code => code.RelativePath == "Calculator.csproj");
        result.Should().ContainSingle(code => code.RelativePath == ".gitignore");
        result.Should().ContainSingle(code => code.RelativePath == "Models\\Add.cs");
        result.Should().ContainSingle(code => code.RelativePath == "Models\\Subtract.cs");
        result.Should().ContainSingle(code => code.RelativePath == "Models\\Divide.cs");
        result.Should().ContainSingle(code => code.RelativePath == "Models\\Multiply.cs");
    }

    [Fact]
    public void LoadApplicationCodes_ShouldIgnoreIgnoredFiles()
    {
        // Arrange
        var service = new CodeLoaderService();

        // Act
        var result = service.LoadApplicationCodes(_appWorkingDir).ToList();

        // Assert
        result.Should().HaveCount(8);
        result.Should().NotContain(code => code.RelativePath == "bin\\Debug\\net8.0\\bin_test.cs");
        result.Should().NotContain(code => code.RelativePath == "obj\\obj_test.cs");
    }

    [Fact]
    public void LoadApplicationCodes_ShouldReturnEmpty_WhenNoValidFilesExist()
    {
        // Arrange
        var service = new CodeLoaderService();

        // Act
        var result = service.LoadApplicationCodes(string.Empty).ToList();

        // Assert
        result.Should().BeEmpty();
    }
}