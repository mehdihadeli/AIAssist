namespace Clients.Models;

public class CodeChange
{
    public string FileRelativePath { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Explanation { get; set; } = default!;
}
