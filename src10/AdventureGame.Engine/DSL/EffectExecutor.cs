namespace AdventureGame.Engine.DSL;

using AdventureGame.Engine.DSL.AST;
using AdventureGame.Engine.DSL.AST.Effects;
using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Elements;
using AdventureGame.Engine.Runtime;

/// <summary>
/// Executor for applying effect AST nodes to game state.
/// </summary>
public class EffectExecutor
{
    /// <summary>
    /// Applies an effect node to the game session.
    /// </summary>
    public void Apply(EffectNode node, GameSession session)
    {
        if (node == null || session == null) return;

        switch (node)
        {
            case SetStateEffect e:
                Apply(e, session);
                break;
            case MoveEffect e:
                Apply(e, session);
                break;
            case InventoryEffect e:
                Apply(e, session);
                break;
            case AttributeEffect e:
                Apply(e, session);
                break;
            case FlagEffect e:
                Apply(e, session);
                break;
            case SayEffect e:
                Apply(e, session);
                break;
        }
    }

    /// <summary>
    /// Applies a SetStateEffect.
    /// </summary>
    private static void Apply(SetStateEffect effect, GameSession session)
    {
        if (effect?.Subject == null) return;

        var target = ResolveTarget(effect.Subject, session);
        if (target == null) return;

        // Set the state
        if (target is GameElement element)
        {
            element.DefaultState = effect.StateName;
        }
    }

    /// <summary>
    /// Applies a MoveEffect.
    /// </summary>
    private static void Apply(MoveEffect effect, GameSession session)
    {
        if (effect?.Subject == null || effect?.Target == null) return;

        var subject = ResolveTarget(effect.Subject, session);
        var targetScene = ResolveTarget(effect.Target, session);

        if (subject == null || targetScene == null) return;

        // Move subject to target scene
        if (subject is GameElement element && targetScene is Scene scene)
        {
            element.ParentId = scene.Id;
        }
    }

    /// <summary>
    /// Applies an InventoryEffect (give, add, remove).
    /// </summary>
    private static void Apply(InventoryEffect effect, GameSession session)
    {
        if (effect?.Item == null || effect?.Subject == null) return;

        var item = ResolveTarget(effect.Item, session);
        var subject = ResolveTarget(effect.Subject, session);

        if (item == null || subject == null) return;

        // Note: Actual inventory implementation depends on your game model
        // This is a placeholder for the game session's inventory management
        switch (effect.Operation?.ToLowerInvariant())
        {
            case "give":
            case "add":
                // session.AddToInventory(subject, item);
                if (item is GameElement itemElement && subject is GameElement subjectElement)
                {
                    itemElement.ParentId = subjectElement.Id;
                }
                break;
            case "remove":
                // session.RemoveFromInventory(subject, item);
                if (item is GameElement itemElement2)
                {
                    itemElement2.ParentId = null;
                }
                break;
        }
    }

    /// <summary>
    /// Applies an AttributeEffect (set, increase, decrease).
    /// </summary>
    private static void Apply(AttributeEffect effect, GameSession session)
    {
        if (effect?.Subject == null || string.IsNullOrWhiteSpace(effect.AttributeName)) return;

        var target = ResolveTarget(effect.Subject, session);
        if (target == null) return;

        if (target is not GameElement element) return;

        var attributeName = effect.AttributeName;

        switch (effect.Operation?.ToLowerInvariant())
        {
            case "set":
                element.Attributes[attributeName] = (int)effect.Value;
                break;
            case "increase":
                if (element.Attributes.TryGetValue(attributeName, out var current))
                {
                    element.Attributes[attributeName] = current + (int)effect.Value;
                }
                else
                {
                    element.Attributes[attributeName] = (int)effect.Value;
                }
                break;
            case "decrease":
                if (element.Attributes.TryGetValue(attributeName, out var currentVal))
                {
                    element.Attributes[attributeName] = currentVal - (int)effect.Value;
                }
                else
                {
                    element.Attributes[attributeName] = -(int)effect.Value;
                }
                break;
        }
    }

    /// <summary>
    /// Applies a FlagEffect.
    /// </summary>
    private static void Apply(FlagEffect effect, GameSession session)
    {
        if (effect?.Subject == null || string.IsNullOrWhiteSpace(effect.FlagName)) return;

        var target = ResolveTarget(effect.Subject, session);
        if (target == null) return;

        if (target is GameElement element)
        {
            element.Flags[effect.FlagName] = effect.Value;
        }
    }

    /// <summary>
    /// Applies a SayEffect (adds message to game log).
    /// </summary>
    private static void Apply(SayEffect effect, GameSession session)
    {
        if (effect == null || string.IsNullOrWhiteSpace(effect.Message)) return;

        // Add to game log or output
        // session.AddToLog(effect.Message);
        // For now, this is a placeholder
    }

    /// <summary>
    /// Resolves a subject reference to an actual game element.
    /// </summary>
    private static object? ResolveTarget(SubjectRef subject, GameSession session)
    {
        if (subject == null || session?.Pack == null) return null;

        return subject.Kind?.ToLowerInvariant() switch
        {
            "player" => session.Player,
            "target" => session.CurrentTarget,
            "item" => session.Pack.Elements.FirstOrDefault(e => e.Id.ToString() == subject.Id || (e is Item && e.Name == subject.Id)),
            "npc" => session.Pack.Elements.FirstOrDefault(e => e.Id.ToString() == subject.Id || (e is Npc && e.Name == subject.Id)),
            "scene" => session.Pack.Elements.FirstOrDefault(e => e.Id.ToString() == subject.Id || (e is Scene && e.Name == subject.Id)),
            "exit" => session.Pack.Elements.FirstOrDefault(e => e.Id.ToString() == subject.Id || (e is Exit && e.Name == subject.Id)),
            _ => null
        };
    }
}
