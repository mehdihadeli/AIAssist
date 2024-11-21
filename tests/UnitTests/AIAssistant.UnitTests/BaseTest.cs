using BuildingBlocks.Utils;

namespace AIAssistant.UnitTests;

public abstract class BaseTest(string appDir) : IAsyncLifetime
{
    protected virtual IList<TestFile> TestFiles { get; } = new List<TestFile>();
    protected string AppDir { get; private set; } = default!;
    protected string WorkingDir { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        WorkingDir = Directory.GetCurrentDirectory();

        // Create the test working directory
        AppDir = Path.Combine(WorkingDir, appDir);
        Directory.CreateDirectory(AppDir);

        // Create the InventoryItem.cs file within the Project directory
        foreach (var testFile in TestFiles)
        {
            string? directoryPath = Path.GetDirectoryName(testFile.Path.NormalizePath());
            if (directoryPath != null) // Check if directory path is not null
            {
                Directory.CreateDirectory(directoryPath);
            }

            await File.WriteAllTextAsync(testFile.Path.NormalizePath(), testFile.Content);
        }
    }

    public async Task DisposeAsync()
    {
        // Clean up the test working directory and files
        if (Directory.Exists(AppDir))
        {
            Directory.Delete(AppDir, recursive: true);
        }

        await Task.CompletedTask;
    }
}
