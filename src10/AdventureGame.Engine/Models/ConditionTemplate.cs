namespace AdventureGame.Engine.Models;

/// <summary>
/// Template for saving and loading sets of conditions for testing.
/// </summary>
public class ConditionTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public List<string> Conditions { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}
