#nullable enable
using AdventureGame.Engine.Models.Round;

namespace AdventureGame.Engine.Models.Actions;

/// <summary>Represents a contiguous dice roll range and its effects.</summary>
public sealed class EffectRange
{
    private int _min = 1;
    private int _max = 20;

    public int MinRoll
    {
        get => _min;
        set => _min = Math.Min(value, _max);
    }

    public int MaxRoll
    {
        get => _max;
        set => _max = Math.Max(value, _min);
    }

    public List<GameEffect> Effects { get; set; } = new();
}

/// <summary>A flat collection of effects to apply without a roll table.</summary>
public sealed class EffectGroup
{
    public List<GameEffect> Effects { get; set; } = new();
}
