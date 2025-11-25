namespace AdventureGame.Engine.Effects;

using AdventureGame.Engine.Semantics;

/// <summary>
/// Base class for all effect AST nodes.
/// </summary>
public abstract class EffectNode
{
    /// <summary>
    /// The subject (target) of the effect. Could be "player", "target", "currentScene", etc.
    /// </summary>
    public string Subject { get; init; } = "";
}

/// <summary>
/// Sets an element's state using semantic resolution.
/// Example: "set target to open"
/// </summary>
public sealed class SetStateEffect : EffectNode
{
    public string StateName { get; init; } = "";
}

/// <summary>
/// Sets a boolean flag on an element.
/// Example: "set target visible to true"
/// </summary>
public sealed class SetFlagEffect : EffectNode
{
    public string FlagName { get; init; } = "";
    public bool Value { get; init; }
}

/// <summary>
/// Sets a string property on an element.
/// Example: "set target description to 'A rusty door'"
/// </summary>
public sealed class SetPropertyEffect : EffectNode
{
    public string PropertyName { get; init; } = "";
    public string Value { get; init; } = "";
}

/// <summary>
/// Sets a numeric attribute on an element.
/// Example: "set player health to 10"
/// </summary>
public sealed class SetAttributeEffect : EffectNode
{
    public string AttributeName { get; init; } = "";
    public int Value { get; init; }
}

/// <summary>
/// Increments a numeric property/attribute.
/// Example: "increment player stamina by 1"
/// </summary>
public sealed class IncrementEffect : EffectNode
{
    public string Key { get; init; } = "";
    public int Amount { get; init; }
}

/// <summary>
/// Decrements a numeric property/attribute.
/// Example: "decrement target battery by 5"
/// </summary>
public sealed class DecrementEffect : EffectNode
{
    public string Key { get; init; } = "";
    public int Amount { get; init; }
}

/// <summary>
/// Displays the description of an element.
/// Example: "describe currentScene"
/// </summary>
public sealed class DescribeEffect : EffectNode
{
}

/// <summary>
/// Prints a literal message to the output.
/// Example: "print 'The door creaks open'"
/// </summary>
public sealed class PrintEffect : EffectNode
{
    public string Message { get; init; } = "";
}

/// <summary>
/// Teleports through an exit.
/// Example: "teleport target" (where target is an exit)
/// </summary>
public sealed class TeleportEffect : EffectNode
{
}

/// <summary>
/// Adds an item to the current player's inventory.
/// Example: "add target to inventory"
/// </summary>
public sealed class AddToInventoryEffect : EffectNode
{
}

/// <summary>
/// Removes an item from the current player's inventory.
/// Example: "remove target from inventory"
/// </summary>
public sealed class RemoveFromInventoryEffect : EffectNode
{
}

/// <summary>
/// Generic semantic set effect that uses the resolver.
/// Example: "set target to open" (resolves "open" semantically)
/// </summary>
public sealed class SetSemanticEffect : EffectNode
{
    public string SemanticName { get; init; } = "";
}
