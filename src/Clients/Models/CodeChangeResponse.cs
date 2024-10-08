namespace Clients.Models;

public class CodeChangeResponse
{
    public IList<CodeChange> CodeChanges { get; set; } = new List<CodeChange>();
}
