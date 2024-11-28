namespace AIAssist.Models.Options;

//The configuration system uses binding and convention-based matching, not JSON deserialization attributes like JsonPropertyName.

public class AppOptions
{
    public string ThemeName { get; set; } = default!;
    public bool PrintCostEnabled { get; set; }
    public string ContextWorkingDirectory { get; set; } = default!;
    public bool AutoContextEnabled { get; set; } = true;
    public IList<string> Files { get; set; } = new List<string>();
    public int NumberOfFilesLimit { get; set; } = 500;
    public int TreeLevel { get; set; }
}
