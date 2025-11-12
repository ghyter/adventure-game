// ==============================
// AdventureGame.Engine/Models/Exit.cs
// ==============================
using System.Collections.ObjectModel;

namespace AdventureGame.Engine.Models.Elements;

/// <summary>
/// Represents a connection between two scenes, such as a door or passage.
/// 
/// An Exit may have an optional <see cref="Direction"/> (for spatial exits)
/// or no direction at all (for exits accessed by name or alias, such as
/// "painting" or "trapdoor").
/// 
/// Each Exit links directly to another Exit via <see cref="TargetExitId"/>.
/// If <see cref="IsSynchronizedStates"/> is true, both exits share their
/// open/closed state — opening one updates the other.
/// 
/// The Exit automatically initializes with two default states:
/// <list type="bullet">
/// <item><term>closed</term> — a closed door</item>
/// <item><term>open</term> — an open door</item>
/// </list>
/// These SVGs can be safely replaced or customized by the editor.
/// The constructor only creates them if no states exist yet.
/// </summary>
public sealed class Exit : GameElement
{
    /// <summary>
    /// The direction this exit represents (North, East, Up, etc.),
    /// or null if the exit is accessed by a custom name/alias.
    /// </summary>
    public Direction? Direction { get; set; }

    /// <summary>
    /// The Exit on the other side of this connection.
    /// This should always point to another Exit element.
    /// </summary>
    public ElementId? TargetExitId { get; set; }

    /// <summary>
    /// When true, this exit’s state changes are mirrored
    /// to its linked target exit (e.g., opening a door affects both sides).
    /// </summary>
    public bool IsSynchronizedStates { get; set; } = true;

    /// <summary>
    /// Creates a new Exit, seeding it with default "open" and "closed" states.
    /// These defaults are only applied if no states already exist.
    /// </summary>
    public Exit()
    {
        if (States.Count == 0)
        {
            States["closed"] = new GameElementState("The exit is closed", ClosedDoorSvg);
            States["open"] = new GameElementState("The exit is open", OpenDoorSvg);
            DefaultState = "closed";
        }
    }

    public override string ToString()
        => Direction is null ? Name : $"{Name} ({Direction})";

    // ============================
    // Default SVG Representations
    // ============================

    /// <summary>
    /// Default "closed" door SVG.
    /// Editors can override this safely by updating the state’s SVG.
    /// </summary>
    private const string ClosedDoorSvg =
        """
        <svg viewBox="0 0 64 64" xmlns="http://www.w3.org/2000/svg">
          <rect x="16" y="8" width="32" height="48" rx="2" ry="2" fill="#8b4513" stroke="#3e1f00" stroke-width="2"/>
          <circle cx="42" cy="32" r="3" fill="#ffd700"/>
        </svg>
        """;

    /// <summary>
    /// Default "open" door SVG.
    /// Editors can override this safely by updating the state’s SVG.
    /// </summary>
    private const string OpenDoorSvg =
        """
        <svg viewBox="0 0 64 64" xmlns="http://www.w3.org/2000/svg">
          <path d="M16 8 L48 8 L40 56 L8 56 Z" fill="#8b4513" stroke="#3e1f00" stroke-width="2"/>
          <circle cx="36" cy="32" r="3" fill="#ffd700"/>
        </svg>
        """;
}
