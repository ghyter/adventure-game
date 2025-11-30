#nullable enable
using System.Text.Json.Serialization;
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

    public List<GameEffect> Effects { get; set; } = [];
}

/// <summary>
/// A group of effects that execute together.
/// Supports sequential or parallel execution modes.
/// </summary>
public sealed class EffectGroup
{
    /// <summary>
    /// Unique identifier for this effect group
    /// </summary>
    [JsonInclude]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// How effects in this group are executed (Sequential or Parallel)
    /// </summary>
    [JsonInclude]
    public ExecutionMode Mode { get; set; } = ExecutionMode.Sequential;
    
    /// <summary>
    /// The effects to execute in this group
    /// </summary>
    [JsonInclude]
    public List<GameEffect> Effects { get; set; } = [];
}
