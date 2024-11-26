namespace AIAssist.Models;

public class CodeBase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SessionId { get; set; }
    public string Code { get; set; } = default!;
    public string RelativeFilePath { get; set; } = default!;
    public string TreeOriginalCode { get; set; } = default!;
}
