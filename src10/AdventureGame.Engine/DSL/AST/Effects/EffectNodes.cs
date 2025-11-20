namespace AdventureGame.Engine.DSL.AST.Effects;

/// <summary>
/// Base class for all effect AST nodes.
/// </summary>
public abstract class EffectNode
{
}

/// <summary>
/// Set state effect: "set <subject>'s state to <state>"
/// </summary>
public sealed class SetStateEffect : EffectNode
{
    public SubjectRef? Subject { get; set; }
    public string StateName { get; set; } = "";
}

/// <summary>
/// Move effect: "move <subject> to <target>"
/// </summary>
public sealed class MoveEffect : EffectNode
{
    public SubjectRef? Subject { get; set; }
    public SubjectRef? Target { get; set; }
}

/// <summary>
/// Inventory effect: "give <item> to <subject>", "add <item> to <subject>", "remove <item> from <subject>"
/// </summary>
public sealed class InventoryEffect : EffectNode
{
    public string Operation { get; set; } = ""; // "give", "add", "remove"
    public SubjectRef? Item { get; set; }
    public SubjectRef? Subject { get; set; }
}

/// <summary>
/// Attribute effect: "set <subject>'s attribute <name> to <value>",
/// "increase <subject>'s attribute <name> by <value>",
/// "decrease <subject>'s attribute <name> by <value>"
/// </summary>
public sealed class AttributeEffect : EffectNode
{
    public SubjectRef? Subject { get; set; }
    public string AttributeName { get; set; } = "";
    public string Operation { get; set; } = ""; // "set", "increase", "decrease"
    public double Value { get; set; }
}

/// <summary>
/// Flag effect: "set <subject>'s flag <name> to true|false"
/// </summary>
public sealed class FlagEffect : EffectNode
{
    public SubjectRef? Subject { get; set; }
    public string FlagName { get; set; } = "";
    public bool Value { get; set; }
}

/// <summary>
/// Say effect: 'say "message"'
/// </summary>
public sealed class SayEffect : EffectNode
{
    public string Message { get; set; } = "";
}
