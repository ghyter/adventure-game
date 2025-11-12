#nullable enable
using AdventureGame.Engine.Models;

namespace AdventureGame.Engine.Models.Actions;

/// <summary>
/// Declares how to select a target element at evaluation time.
/// You can specify by ElementId, by element Kind, Alias, or logical Role.
/// </summary>
public sealed class TargetSelector
{
    public ElementId? ElementId { get; set; }
    public string? Kind { get; set; }
    public string? Alias { get; set; }
    /// <summary>Logical role to associate with this target (e.g., "Target1", "Target2").</summary>
    public string? Role { get; set; }
}
