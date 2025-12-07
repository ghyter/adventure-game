using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Runtime;

namespace AdventureGame.Engine.Parameters;

/// <summary>
/// Resolves element references (names, aliases, or special variables) to actual GameElement instances.
/// </summary>
public static class ElementReferenceResolver
{
    /// <summary>
    /// Resolves an element reference to a GameElement instance.
    /// Supports: element names, aliases, IDs, and special variables (target, target1, target2, currentScene, currentPlayer)
    /// </summary>
    public static GameElement? ResolveElement(
        string? reference,
        GameRound round,
        GameSession session)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return null;

        reference = reference.Trim();

        // Handle special variables first
        var element = ResolveSpecialVariable(reference, round, session);
        if (element != null)
            return element;

        // Try to find by exact ID match first (implicit string conversion)
        element = session.Elements.FirstOrDefault(e => ((string)e.Id).Equals(reference, StringComparison.Ordinal));
        if (element != null)
            return element;

        // Search by name (case-insensitive)
        element = session.Elements.FirstOrDefault(e => 
            e.Name.Equals(reference, StringComparison.OrdinalIgnoreCase));
        if (element != null)
            return element;

        // Search by alias (case-insensitive)
        element = session.Elements.FirstOrDefault(e => 
            e.Aliases.Contains(reference));
        if (element != null)
            return element;

        return null;
    }

    /// <summary>
    /// Resolves special variable references to GameElement instances.
    /// </summary>
    private static GameElement? ResolveSpecialVariable(
        string reference,
        GameRound round,
        GameSession session)
    {
        return reference.ToLowerInvariant() switch
        {
            "target" or "target1" => round.Target1,
            "target2" => round.Target2,
            "currentscene" or "scene" => session.CurrentScene,
            "currentplayer" or "player" => session.Player,
            _ => null
        };
    }

    /// <summary>
    /// Gets all available element references for a given session and round.
    /// Useful for providing autocomplete suggestions in editors.
    /// </summary>
    public static List<ElementReference> GetAvailableReferences(
        GameRound? round,
        GameSession session)
    {
        var references = new List<ElementReference>();

        // Add special variables
        if (round != null)
        {
            if (round.Target1 != null)
            {
                references.Add(new ElementReference 
                { 
                    DisplayName = "target (or target1)", 
                    Value = "target",
                    Description = $"Current target: {round.Target1.Name}",
                    Category = "Special Variables"
                });
            }

            if (round.Target2 != null)
            {
                references.Add(new ElementReference 
                { 
                    DisplayName = "target2", 
                    Value = "target2",
                    Description = $"Second target: {round.Target2.Name}",
                    Category = "Special Variables"
                });
            }
        }

        if (session.CurrentScene != null)
        {
            references.Add(new ElementReference 
            { 
                DisplayName = "currentScene (or scene)", 
                Value = "currentScene",
                Description = $"Current scene: {session.CurrentScene.Name}",
                Category = "Special Variables"
            });
        }

        if (session.Player != null)
        {
            references.Add(new ElementReference 
            { 
                DisplayName = "currentPlayer (or player)", 
                Value = "currentPlayer",
                Description = $"Player: {session.Player.Name}",
                Category = "Special Variables"
            });
        }

        // Add all elements by name
        foreach (var element in session.Elements)
        {
            references.Add(new ElementReference 
            { 
                DisplayName = element.Name, 
                Value = element.Name,
                Description = $"{element.Kind}: {element.Description}",
                Category = "Elements by Name"
            });

            // Add aliases
            foreach (var alias in element.Aliases)
            {
                references.Add(new ElementReference 
                { 
                    DisplayName = $"{alias} (alias for {element.Name})", 
                    Value = alias,
                    Description = $"{element.Kind}: {element.Description}",
                    Category = "Element Aliases"
                });
            }
        }

        return references;
    }
}

/// <summary>
/// Represents an element reference that can be used in parameter editors.
/// </summary>
public class ElementReference
{
    public string DisplayName { get; set; } = "";
    public string Value { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
}
