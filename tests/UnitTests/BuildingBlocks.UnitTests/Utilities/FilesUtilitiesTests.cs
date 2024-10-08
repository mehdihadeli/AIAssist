namespace BuildingBlocks.UnitTests.Utilities;

using FluentAssertions;
using Utils;
using Xunit;

public class FilesUtilitiesTests : IAsyncLifetime
{
    private string _testDirectory = default!;
    private string _originalCurrentDirectory = default!;

    public Task InitializeAsync()
    {
        // Store the original current directory
        _originalCurrentDirectory = Environment.CurrentDirectory;

        // Create a test directory for all tests
        _testDirectory = Path.Combine(_originalCurrentDirectory, "TestFiles");

        File.Copy(Path.Combine(_originalCurrentDirectory, ".gitignore"), $"/{_testDirectory}");

        Directory.CreateDirectory(_testDirectory);

        // Set the test directory as the current directory
        Environment.CurrentDirectory = _testDirectory;

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        // Reset the current directory
        Environment.CurrentDirectory = _originalCurrentDirectory;

        // Delete the test directory and its contents
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }

        return Task.CompletedTask;
    }

    [Fact]
    public void Test_IgnoredBinDirectory()
    {
        // Arrange: simulate a file in the bin directory
        string binFolderPath = Path.Combine("bin", "somefile.txt"); // Use relative path
        Directory.CreateDirectory(Path.Combine("bin"));
        File.Create(binFolderPath).Dispose(); // Create the file

        // Act
        bool isIgnored = FilesUtilities.IsIgnored(binFolderPath);

        // Assert
        isIgnored.Should().BeTrue();
    }

    [Fact]
    public void Test_IgnoredBinWithDifferentCase()
    {
        // Arrange: simulate a file in the Bin directory
        string binFolderPath = Path.Combine("Bin", "somefile.txt"); // Use relative path
        Directory.CreateDirectory(Path.Combine("Bin"));
        File.Create(binFolderPath).Dispose(); // Create the file

        // Act
        bool isIgnored = FilesUtilities.IsIgnored(binFolderPath);

        // Assert
        isIgnored.Should().BeTrue();
    }

    [Fact]
    public void Test_IgnoredBinPatternFromGitignore()
    {
        // Arrange: simulate a file in the bin directory
        string binFolderPath = Path.Combine("bin", "somefile.txt"); // Use relative path
        Directory.CreateDirectory(Path.Combine("bin"));
        File.Create(binFolderPath).Dispose(); // Create the file

        // Act
        bool isIgnored = FilesUtilities.IsIgnored(binFolderPath);

        // Assert
        isIgnored.Should().BeTrue();

        // Test a file in the Bin directory (different case)
        string binFolderPathDifferentCase = Path.Combine("Bin", "somefile.txt"); // Use relative path
        Directory.CreateDirectory(Path.Combine("Bin"));
        File.Create(binFolderPathDifferentCase).Dispose(); // Create the file

        // Act
        bool isIgnoredDifferentCase = FilesUtilities.IsIgnored(binFolderPathDifferentCase);

        // Assert
        isIgnoredDifferentCase.Should().BeTrue();
    }

    [Fact]
    public void Test_NotIgnoredFileOutsideBinDirectory()
    {
        // Arrange: simulate a file outside the bin directory
        string filePath = Path.Combine("src", "somefile.txt"); // Use relative path
        Directory.CreateDirectory(Path.Combine("src"));
        File.Create(filePath).Dispose(); // Create the file

        // Act
        bool isIgnored = FilesUtilities.IsIgnored(filePath);

        // Assert
        isIgnored.Should().BeFalse();
    }

    [Fact]
    public void Test_IgnoredObjDirectory()
    {
        // Arrange: simulate a file in the obj directory
        string objFolderPath = Path.Combine("obj", "somefile.txt"); // Use relative path
        Directory.CreateDirectory(Path.Combine("obj"));
        File.Create(objFolderPath).Dispose(); // Create the file

        // Act
        bool isIgnored = FilesUtilities.IsIgnored(objFolderPath);

        // Assert
        isIgnored.Should().BeTrue();
    }

    [Fact]
    public void Test_IgnoredObjWithDifferentCase()
    {
        // Arrange: simulate a file in the Obj directory
        string objFolderPath = Path.Combine("Obj", "somefile.txt"); // Use relative path
        Directory.CreateDirectory(Path.Combine("Obj"));
        File.Create(objFolderPath).Dispose(); // Create the file

        // Act
        bool isIgnored = FilesUtilities.IsIgnored(objFolderPath);

        // Assert
        isIgnored.Should().BeTrue();
    }

    [Fact]
    public void Test_IgnoredNodeModulesDirectory()
    {
        // Arrange: simulate a file in the node_modules directory
        string nodeModulesPath = Path.Combine("node_modules", "somepackage"); // Use relative path
        Directory.CreateDirectory(Path.Combine("node_modules"));

        // Act
        bool isIgnored = FilesUtilities.IsIgnored(nodeModulesPath);

        // Assert
        isIgnored.Should().BeTrue();
    }

    [Fact]
    public void Test_IgnoredTempDirectory()
    {
        // Arrange: simulate a file in the temp directory
        string tempPath = Path.Combine("temp", "tempfile.txt"); // Use relative path
        Directory.CreateDirectory(Path.Combine("temp"));
        File.Create(tempPath).Dispose(); // Create the file

        // Act
        bool isIgnored = FilesUtilities.IsIgnored(tempPath);

        // Assert
        isIgnored.Should().BeTrue();
    }

    [Fact]
    public void Test_NotIgnoredFileInValidDirectory()
    {
        // Arrange: simulate a file in a valid directory that is not ignored
        string validFilePath = Path.Combine("src", "notIgnoredFile.txt"); // Use relative path
        Directory.CreateDirectory(Path.Combine("src"));
        File.Create(validFilePath).Dispose(); // Create the file

        // Act
        bool isIgnored = FilesUtilities.IsIgnored(validFilePath);

        // Assert
        isIgnored.Should().BeFalse();
    }

    [Fact]
    public void Test_IgnoredDotVscodeDirectory()
    {
        // Arrange: simulate a file in the .vscode directory
        string vscodePath = Path.Combine(".vscode", "settings.json"); // Use relative path
        Directory.CreateDirectory(Path.Combine(".vscode"));
        File.Create(vscodePath).Dispose(); // Create the file

        // Act
        bool isIgnored = FilesUtilities.IsIgnored(vscodePath);

        // Assert
        isIgnored.Should().BeTrue();
    }

    [Fact]
    public void Test_IgnoredLogsDirectory()
    {
        // Arrange: simulate a file in the logs directory
        string logsPath = Path.Combine("logs", "logfile.log"); // Use relative path
        Directory.CreateDirectory(Path.Combine("logs"));
        File.Create(logsPath).Dispose(); // Create the file

        // Act
        bool isIgnored = FilesUtilities.IsIgnored(logsPath);

        // Assert
        isIgnored.Should().BeTrue();
    }

    [Fact]
    public void Test_IgnoredDotEnvFile()
    {
        // Arrange: simulate a file in the env directory
        string envPath = Path.Combine(".env"); // Use relative path
        File.Create(envPath).Dispose(); // Create the file

        // Act
        bool isIgnored = FilesUtilities.IsIgnored(envPath);

        // Assert
        isIgnored.Should().BeTrue();
    }

    [Fact]
    public void Test_NotIgnoredFileWithAllowedPrefix()
    {
        // Arrange: simulate a file with an allowed prefix
        string allowedPrefixPath = Path.Combine("src", ".keep"); // Use relative path
        Directory.CreateDirectory(Path.Combine("src"));
        File.Create(allowedPrefixPath).Dispose(); // Create the file

        // Act
        bool isIgnored = FilesUtilities.IsIgnored(allowedPrefixPath);

        // Assert
        isIgnored.Should().BeFalse();
    }

    [Fact]
    public void Test_IgnoredCacheDirectory()
    {
        // Arrange: simulate a file in the cache directory
        string cachePath = Path.Combine("cache", "cachedfile.cache"); // Use relative path
        Directory.CreateDirectory(Path.Combine("cache"));
        File.Create(cachePath).Dispose(); // Create the file

        // Act
        bool isIgnored = FilesUtilities.IsIgnored(cachePath);

        // Assert
        isIgnored.Should().BeTrue();
    }
}
