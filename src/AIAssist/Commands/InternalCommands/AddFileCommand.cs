using AIAssist.Models.Options;
using BuildingBlocks.SpectreConsole.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace AIAssist.Commands.InternalCommands;

public class AddFileCommand(ISpectreUtilities spectreUtilities, AppOptions appOptions) : IInternalConsoleCommand
{
    public string Name => AIAssistConstants.InternalCommands.AddFiles;
    public string Command => $":{Name}";
    public string? ShortCommand => ":a";
    public ConsoleKey? ShortcutKey => ConsoleKey.A;
    public bool IsDefaultCommand => false;

    public Task<bool> ExecuteAsync(IServiceScope scope, string? input)
    {
        var args = spectreUtilities.GetArguments(input);
        var filesToAdd = new List<string>();
        var matcher = new Matcher();

        foreach (var path in args)
        {
            if (Directory.Exists(path))
            {
                // Add all files from the directory recursively
                var directoryFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                filesToAdd.AddRange(directoryFiles);
            }
            else if (path.Contains('*', StringComparison.Ordinal))
            {
                matcher.AddInclude(path);
            }
            else if (File.Exists(path))
            {
                filesToAdd.Add(path);
            }
            else
            {
                spectreUtilities.ErrorTextLine($"The specified path does not exist: {path}");
            }
        }

        var directoryInfo = new DirectoryInfo(appOptions.ContextWorkingDirectory);
        var directoryInfoWrapper = new DirectoryInfoWrapper(directoryInfo);

        var results = matcher.Execute(directoryInfoWrapper);
        filesToAdd.AddRange(results.Files.Select(f => f.Path));

        foreach (var fileToAdd in filesToAdd.Distinct())
        {
            if (!appOptions.Files.Contains(fileToAdd))
            {
                appOptions.Files.Add(fileToAdd);
            }
        }

        spectreUtilities.InformationTextLine(
            filesToAdd.Count != 0 ? $"Files added: {string.Join(", ", filesToAdd)}" : "No files were added."
        );

        return Task.FromResult(true);
    }
}
