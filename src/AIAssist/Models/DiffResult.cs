namespace AIAssist.Models;

public class DiffResult
{
    public string OriginalPath { get; set; } = default!;
    public string ModifiedPath { get; set; } = default!;
    public string Language { get; set; } = default!;
    public IList<Replacement>? Replacements { get; set; }
    public ActionType Action { get; set; }
    public IList<string>? ModifiedLines { get; set; } = new List<string>();
}
