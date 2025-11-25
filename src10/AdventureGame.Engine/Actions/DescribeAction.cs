namespace AdventureGame.Engine.Actions;

using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Elements;
using AdventureGame.Engine.Runtime;
using AdventureGame.Engine.Effects;
using AdventureGame.Engine.Semantics;

public class ActionExecutor
{
    private readonly SemanticPropertyResolver _resolver = new();

    public string ExecuteDescribe(GameElement element)
    {
        // Get current state description
        var stateDescription = "";
        if (element.Properties.TryGetValue("CurrentState", out var currentState) 
            && !string.IsNullOrWhiteSpace(currentState)
            && element.States.TryGetValue(currentState, out var state))
        {
            stateDescription = state.Description;
        }
        else if (element.States.TryGetValue(element.DefaultState, out var defaultState))
        {
            stateDescription = defaultState.Description;
        }

        return $"{element.Name}\n{element.Description}\n{stateDescription}";
    }

    /// <summary>
    /// Executes a natural language effect node on the given session.
    /// </summary>
    public string Execute(EffectNode effect, GameSession session, GameElement? target, GameElement? target2)
    {
        if (effect == null || session == null)
            return "Error: Invalid effect or session";

        return effect switch
        {
            SetSemanticEffect setSemantic => ExecuteSetSemantic(setSemantic, session, target, target2),
            SetStateEffect setState => ExecuteSetState(setState, session, target, target2),
            SetFlagEffect setFlag => ExecuteSetFlag(setFlag, session, target, target2),
            SetPropertyEffect setProperty => ExecuteSetProperty(setProperty, session, target, target2),
            SetAttributeEffect setAttribute => ExecuteSetAttribute(setAttribute, session, target, target2),
            IncrementEffect increment => ExecuteIncrement(increment, session, target, target2),
            DecrementEffect decrement => ExecuteDecrement(decrement, session, target, target2),
            DescribeEffect describe => ExecuteDescribeEffect(describe, session, target, target2),
            PrintEffect print => print.Message,
            TeleportEffect teleport => ExecuteTeleport(teleport, session, target, target2),
            AddToInventoryEffect addInventory => ExecuteAddToInventory(addInventory, session, target, target2),
            RemoveFromInventoryEffect removeInventory => ExecuteRemoveFromInventory(removeInventory, session, target, target2),
            _ => "Unknown effect type"
        };
    }

    private GameElement? ResolveSubject(string subject, GameSession session, GameElement? target, GameElement? target2)
    {
        return subject.ToLowerInvariant() switch
        {
            "player" or "currentplayer" => session.Player,
            "currentscene" => session.CurrentScene,
            "target" or "target1" => target,
            "target2" => target2,
            _ => session.Elements.FirstOrDefault(e => 
                e.Name.Equals(subject, StringComparison.OrdinalIgnoreCase) ||
                e.Aliases.Contains(subject, StringComparer.OrdinalIgnoreCase))
        };
    }

    private string ExecuteSetSemantic(SetSemanticEffect effect, GameSession session, GameElement? target, GameElement? target2)
    {
        var element = ResolveSubject(effect.Subject, session, target, target2);
        if (element == null)
            return $"Error: Could not find element '{effect.Subject}'";

        var binding = _resolver.Resolve(element, effect.SemanticName);
        if (binding == null)
        {
            // If no existing property, assume it's a state
            element.Properties["CurrentState"] = effect.SemanticName;
            return $"{element.Name} is now {effect.SemanticName}.";
        }

        _resolver.SetValue(element, binding, effect.SemanticName);
        return $"{element.Name} is now {effect.SemanticName}.";
    }

    private string ExecuteSetState(SetStateEffect effect, GameSession session, GameElement? target, GameElement? target2)
    {
        var element = ResolveSubject(effect.Subject, session, target, target2);
        if (element == null)
            return $"Error: Could not find element '{effect.Subject}'";

        element.Properties["CurrentState"] = effect.StateName;
        return $"{element.Name} is now {effect.StateName}.";
    }

    private string ExecuteSetFlag(SetFlagEffect effect, GameSession session, GameElement? target, GameElement? target2)
    {
        var element = ResolveSubject(effect.Subject, session, target, target2);
        if (element == null)
            return $"Error: Could not find element '{effect.Subject}'";

        element.Flags[effect.FlagName] = effect.Value;
        return $"{element.Name}.{effect.FlagName} is now {effect.Value}.";
    }

    private string ExecuteSetProperty(SetPropertyEffect effect, GameSession session, GameElement? target, GameElement? target2)
    {
        var element = ResolveSubject(effect.Subject, session, target, target2);
        if (element == null)
            return $"Error: Could not find element '{effect.Subject}'";

        element.Properties[effect.PropertyName] = effect.Value;
        return $"{element.Name}.{effect.PropertyName} is now '{effect.Value}'.";
    }

    private string ExecuteSetAttribute(SetAttributeEffect effect, GameSession session, GameElement? target, GameElement? target2)
    {
        var element = ResolveSubject(effect.Subject, session, target, target2);
        if (element == null)
            return $"Error: Could not find element '{effect.Subject}'";

        element.Attributes[effect.AttributeName] = effect.Value;
        return $"{element.Name}.{effect.AttributeName} is now {effect.Value}.";
    }

    private string ExecuteIncrement(IncrementEffect effect, GameSession session, GameElement? target, GameElement? target2)
    {
        var element = ResolveSubject(effect.Subject, session, target, target2);
        if (element == null)
            return $"Error: Could not find element '{effect.Subject}'";

        var binding = _resolver.Resolve(element, effect.Key);
        if (binding != null && _resolver.Increment(element, binding, effect.Amount))
        {
            var newValue = _resolver.GetValue(element, binding);
            return $"{element.Name}.{effect.Key} increased by {effect.Amount} to {newValue}.";
        }

        return $"Error: Could not increment {effect.Key} on {element.Name}";
    }

    private string ExecuteDecrement(DecrementEffect effect, GameSession session, GameElement? target, GameElement? target2)
    {
        var element = ResolveSubject(effect.Subject, session, target, target2);
        if (element == null)
            return $"Error: Could not find element '{effect.Subject}'";

        var binding = _resolver.Resolve(element, effect.Key);
        if (binding != null && _resolver.Decrement(element, binding, effect.Amount))
        {
            var newValue = _resolver.GetValue(element, binding);
            return $"{element.Name}.{effect.Key} decreased by {effect.Amount} to {newValue}.";
        }

        return $"Error: Could not decrement {effect.Key} on {element.Name}";
    }

    private string ExecuteDescribeEffect(DescribeEffect effect, GameSession session, GameElement? target, GameElement? target2)
    {
        var element = ResolveSubject(effect.Subject, session, target, target2);
        if (element == null)
            return $"Error: Could not find element '{effect.Subject}'";

        return ExecuteDescribe(element);
    }

    private string ExecuteTeleport(TeleportEffect effect, GameSession session, GameElement? target, GameElement? target2)
    {
        var exitElement = ResolveSubject(effect.Subject, session, target, target2);
        if (exitElement == null)
            return $"Error: Could not find element '{effect.Subject}'";

        if (exitElement is not Exit exit)
            return $"Error: {exitElement.Name} is not an exit";

        var pairedExit = session.Elements.OfType<Exit>()
            .FirstOrDefault(e => e.Id == exit.TargetExitId);

        if (pairedExit == null)
            return $"Error: Exit {exit.Name} has no paired exit";

        var targetScene = session.Elements.OfType<Scene>()
            .FirstOrDefault(s => s.Id == pairedExit.ParentId);

        if (targetScene == null)
            return $"Error: Could not find destination scene";

        if (session.Player != null)
        {
            session.Player.ParentId = targetScene.Id;
            session.CurrentScene = targetScene;
            return $"You travel through {exit.Name} and arrive at {targetScene.Name}.";
        }

        return "Error: No player to teleport";
    }

    private string ExecuteAddToInventory(AddToInventoryEffect effect, GameSession session, GameElement? target, GameElement? target2)
    {
        var item = ResolveSubject(effect.Subject, session, target, target2);
        if (item == null)
            return $"Error: Could not find item '{effect.Subject}'";

        if (session.Player == null)
            return "Error: No player to add item to";

        item.ParentId = session.Player.Id;
        return $"{item.Name} added to inventory.";
    }

    private string ExecuteRemoveFromInventory(RemoveFromInventoryEffect effect, GameSession session, GameElement? target, GameElement? target2)
    {
        var item = ResolveSubject(effect.Subject, session, target, target2);
        if (item == null)
            return $"Error: Could not find item '{effect.Subject}'";

        if (session.Player == null)
            return "Error: No player";

        if (session.CurrentScene != null)
        {
            item.ParentId = session.CurrentScene.Id;
            return $"{item.Name} removed from inventory.";
        }

        return "Error: No current scene to drop item";
    }
}
