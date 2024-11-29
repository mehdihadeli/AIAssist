namespace BuildingBlocks.UnitTests.Utilities;

using FluentAssertions;
using Utils;
using Xunit;

public class FilesUtilitiesTests : IAsyncLifetime
{
    private string _appWorkingDir = default!;
    private string _originalWorkingDir = default!;
    private string _binDir = default!;
    private string _objDir = default!;

    public async Task InitializeAsync()
    {
        _originalWorkingDir = Directory.GetCurrentDirectory();

        // Save the original working directory
        _appWorkingDir = Path.Combine(Directory.GetCurrentDirectory(), "TestData/Calculator");

        // Change the working directory to the new test directory
        Directory.SetCurrentDirectory(_appWorkingDir);

        _binDir = Path.Combine(_appWorkingDir, "bin");
        _objDir = Path.Combine(_appWorkingDir, "obj");

        if (!Directory.Exists(_binDir))
        {
            Directory.CreateDirectory(_binDir);
        }

        if (!Directory.Exists(_objDir))
        {
            Directory.CreateDirectory(_objDir);
        }

        await Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Directory.SetCurrentDirectory(_originalWorkingDir);
        return Task.CompletedTask;
    }

    // [Theory]
    // [InlineData("bin/test.txt", true)] // Should be ignored
    // [InlineData("bin", true)] // Should be ignored
    // [InlineData("obj/test.txt", true)] // Should be ignored
    // [InlineData("src/main.cs", false)] // Should not be ignored
    // [InlineData("docs/readme.md", false)] // Should not be ignored
    // public void Test_IgnoredDirectoriesAndFiles(string path, bool expected)
    // {
    //     // Act
    //     bool isIgnored = FilesUtilities.IsIgnored(path);
    //
    //     // Assert
    //     isIgnored.Should().Be(expected);
    // }
    //
    // [Theory]
    // [InlineData("bin", true)] // Normal case
    // [InlineData("BIN", true)] // Uppercase
    // [InlineData("Bin", true)] // Mixed case
    // [InlineData("BiN/test.dll", true)] // Mixed case with file
    // [InlineData("bin/test.dll", true)] // Normal case with file
    // [InlineData("src/main.cs", false)] // Source directory, not ignored
    // public void Test_IgnoredBinWithDifferentCase(string path, bool expected)
    // {
    //     // Act
    //     bool isIgnored = FilesUtilities.IsIgnored(path);
    //
    //     // Assert
    //     isIgnored.Should().Be(expected);
    // }
    //
    // [Theory]
    // [InlineData("obj", true)] // The obj directory itself should be ignored
    // [InlineData("obj/Debug/test.dll", true)] // Files inside obj directory should be ignored
    // [InlineData("obj/Release/test.dll", true)] // Files inside obj directory should be ignored
    // [InlineData("src/main.cs", false)] // Should not be ignored (valid source folder)
    // [InlineData("docs/readme.md", false)] // Should not be ignored (documentation folder)
    // public void Test_IgnoredObjDirectory(string path, bool expected)
    // {
    //     // Act
    //     bool isIgnored = FilesUtilities.IsIgnored(path);
    //
    //     // Assert
    //     isIgnored.Should().Be(expected);
    // }
    //
    // [Theory]
    // [InlineData("obj", true)] // Normal case
    // [InlineData("OBJ", true)] // Uppercase
    // [InlineData("Obj", true)] // Mixed case
    // [InlineData("oBj/Debug/test.dll", true)] // Mixed case with a file path inside obj
    // [InlineData("obj/test.dll", true)] // Normal case with a file inside obj
    // [InlineData("src/main.cs", false)] // Source directory, should not be ignored
    // [InlineData("docs/readme.md", false)] // Documentation file, should not be ignored
    // public void Test_IgnoredObjWithDifferentCase(string path, bool expected)
    // {
    //     // Act
    //     bool isIgnored = FilesUtilities.IsIgnored(path);
    //
    //     // Assert
    //     isIgnored.Should().Be(expected);
    // }
    //
    // [Theory]
    // [InlineData("bin", true)] // Root bin directory
    // [InlineData("bin/", true)] // Explicit directory pattern with a trailing slash
    // [InlineData("bin/test.dll", true)] // File inside the bin directory
    // [InlineData("bin/Debug/test.dll", true)] // File inside a subdirectory of bin
    // [InlineData("src/bin/test.dll", false)] // A bin directory inside another path, should not be ignored
    // [InlineData("binExtra/test.dll", false)] // Similar name but different, should not be ignored
    // public void Test_IgnoredBinPatternFromGitignore(string path, bool expected)
    // {
    //     // Act
    //     bool isIgnored = FilesUtilities.IsIgnored(path);
    //
    //     // Assert
    //     isIgnored.Should().Be(expected);
    // }
    //
    // [Theory]
    // [InlineData("src/main.cs", false)] // Source file outside of bin
    // [InlineData("docs/readme.md", false)] // Documentation file outside of bin
    // [InlineData("assets/image.png", false)] // Image file outside of bin
    // [InlineData("config/settings.json", false)] // Configuration file outside of bin
    // [InlineData("binTest/test.dll", false)] // Directory similar to bin but should not be ignored
    // [InlineData("README.md", false)] // Project root file
    // public void Test_NotIgnoredFileOutsideBinDirectory(string path, bool expected)
    // {
    //     // Act
    //     bool isIgnored = FilesUtilities.IsIgnored(path);
    //
    //     // Assert
    //     isIgnored.Should().Be(expected);
    // }
    //
    // [Theory]
    // [InlineData("node_modules", true)] // The node_modules directory itself should be ignored
    // [InlineData("node_modules/", true)] // Explicit directory pattern with a trailing slash
    // [InlineData("node_modules/package.json", true)] // A file inside the node_modules directory
    // [InlineData("node_modules/react/index.js", true)] // A file in a subdirectory of node_modules
    // [InlineData("src/node_modules/test.js", false)] // A node_modules directory inside another path, should not be ignored
    // [InlineData("README.md", false)] // A file in the root that should not be ignored
    // public void Test_IgnoredNodeModulesDirectory(string path, bool expected)
    // {
    //     // Act
    //     bool isIgnored = FilesUtilities.IsIgnored(path);
    //
    //     // Assert
    //     isIgnored.Should().Be(expected);
    // }
    //
    // [Theory]
    // [InlineData("temp", true)] // The temp directory itself should be ignored
    // [InlineData("temp/", true)] // Explicit directory pattern with a trailing slash
    // [InlineData("temp/file.tmp", true)] // A file inside the temp directory
    // [InlineData("temp/session/anotherfile.tmp", true)] // A file in a subdirectory of temp
    // [InlineData("src/temp/test.tmp", false)] // A temp directory inside another path, should not be ignored
    // [InlineData("tempFiles/somefile.txt", false)] // A similar but different directory, should not be ignored
    // [InlineData("README.md", false)] // A file in the root that should not be ignored
    // public void Test_IgnoredTempDirectory(string path, bool expected)
    // {
    //     // Act
    //     bool isIgnored = FilesUtilities.IsIgnored(path);
    //
    //     // Assert
    //     isIgnored.Should().Be(expected);
    // }
    //
    // [Theory]
    // [InlineData("Models/Add.cs", false)] // Checking Add.cs file in Models folder
    // [InlineData("Models/Subtract.cs", false)] // Checking Subtract.cs file in Models folder
    // [InlineData("Models/Multiply.cs", false)] // Checking Multiply.cs file in Models folder
    // [InlineData("Models/Divide.cs", false)] // Checking Divide.cs file in Models folder
    // [InlineData("Program.cs", false)] // Checking Program.cs file in the root
    // [InlineData("Calculator.csproj", false)] // Checking Calculator.csproj in the root
    // public void Test_NotIgnoredFileInValidDirectory(string path, bool expected)
    // {
    //     // Act
    //     bool isIgnored = FilesUtilities.IsIgnored(path);
    //
    //     // Assert
    //     isIgnored.Should().Be(expected);
    // }
    //
    // [Theory]
    // [InlineData("logs", true)] // The logs directory itself should be ignored
    // [InlineData("logs/", true)] // Explicit directory pattern with a trailing slash
    // [InlineData("logs/application.log", true)] // A log file inside the logs directory
    // [InlineData("logs/2024-10-09.log", true)] // Another log file with a date pattern
    // [InlineData("src/logs/debug.log", false)] // A logs directory inside another path (should not be ignored)
    // [InlineData("temporaryLogs/test.log", false)] // A different directory that should not be ignored
    // [InlineData("README.md", false)] // A file that should not be ignored
    // public void Test_IgnoredLogsDirectory(string path, bool expected)
    // {
    //     // Act
    //     bool isIgnored = FilesUtilities.IsIgnored(path);
    //
    //     // Assert
    //     isIgnored.Should().Be(expected);
    // }
    //
    // [Theory]
    // [InlineData(".env", true)] // The root .env file should be ignored
    // [InlineData(".env.example", false)] // An example env file, should not be ignored
    // [InlineData("config/.env", true)] // A .env file inside a config directory should be ignored
    // [InlineData("src/.env", true)] // Another .env file in a source folder
    // [InlineData("data/.env.backup", false)] // A backup file that should not be ignored
    // [InlineData("temp/.env.tmp", true)] // A temporary .env file that should be ignored
    // [InlineData("README.md", false)] // A file that is not a .env file and should not be ignored
    // public void Test_IgnoredDotEnvFile(string path, bool expected)
    // {
    //     // Act
    //     bool isIgnored = FilesUtilities.IsIgnored(path);
    //
    //     // Assert
    //     isIgnored.Should().Be(expected);
    // }
    //
    // [Theory]
    // [InlineData(".keep", false)]
    // [InlineData("src/.keep", false)]
    // public void Test_NotIgnoredFileWithAllowedPrefix(string path, bool expected)
    // {
    //     // Act
    //     bool isIgnored = FilesUtilities.IsIgnored(path);
    //
    //     // Assert
    //     isIgnored.Should().Be(expected);
    // }
    //
    // [Theory]
    // [InlineData("cache", true)] // The cache directory itself should be ignored
    // [InlineData("cache/", true)] // Explicit directory pattern with a trailing slash
    // [InlineData("cache/temp.txt", true)] // A file inside the cache directory should be ignored
    // [InlineData("cache/images/image1.png", true)] // An image file inside a cache subdirectory should be ignored
    // [InlineData("src/cache/temp.log", false)] // A cache directory inside src should not be ignored
    // [InlineData("temporaryCache/test.txt", false)] // A different directory that should not be ignored
    // [InlineData("README.md", false)] // A file that is not in any cache directory and should not be ignored
    // public void Test_IgnoredCacheDirectory(string path, bool expected)
    // {
    //     // Act
    //     bool isIgnored = FilesUtilities.IsIgnored(path);
    //
    //     // Assert
    //     isIgnored.Should().Be(expected);
    // }
}
