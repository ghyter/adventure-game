namespace AdventureGame.Engine.Models.Round;

using AdventureGame.Engine.Models.Actions;

// -----------------------------
// Triggers
// -----------------------------
public sealed class Trigger : GameAction
{
    public bool FireOnce { get; set; } = true;
    public bool Fired { get; set; } = false;

    // Structured authoring additions
    public ConditionGroup Conditions { get; set; } = new();
    public List<EffectRange> EffectRanges { get; set; } = new();
    public List<ModifierSource> Modifiers { get; set; } = new();
}