namespace AIAssistant.Models.Options;

public class AppOptions
{
    public string ThemeName { get; set; } = "dracula";
    public bool PrintCostEnabled { get; set; }
    public string ContextWorkingDirectory { get; set; } = default!;
    public bool AutoContextEnabled { get; set; } = true;
    public IList<string> Files { get; set; } = new List<string>();
}
