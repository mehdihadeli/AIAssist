using System.Diagnostics;

namespace AIRefactorAssistant.Utils;

public static class Installer
{
    public static void InstallGlow()
    {
        if (!IsGoInstalled())
        {
            Console.WriteLine("Go is not installed. Installing Go...");
            return;
        }

        var processInfo = new ProcessStartInfo
        {
            FileName = "go",
            Arguments = "install github.com/charmbracelet/glow@latest",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(processInfo);

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        Console.WriteLine(output);
        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine("Error: " + error);
        }
    }

    private static bool IsGoInstalled()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "go",
                Arguments = "version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (var process = Process.Start(processInfo))
            {
                process.WaitForExit();
                return process.ExitCode == 0;
            }
        }
        catch
        {
            return false;
        }
    }
}
