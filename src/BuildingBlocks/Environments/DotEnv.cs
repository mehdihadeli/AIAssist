namespace BuildingBlocks.Environments;

public static class DotEnv
{
    public static void Load(string directory)
    {
        var dotenv = Path.Combine(directory, ".env");

        if (!File.Exists(dotenv))
            return;

        foreach (var line in File.ReadAllLines(dotenv))
        {
            var parts = line.Split('=', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                continue;

            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }

    public static void LoadCurrentDirectory()
    {
        var root = Directory.GetCurrentDirectory();
        Load(root);
    }
}
