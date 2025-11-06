// ==============================
// AdventureGame.Engine/Models/Item.cs
// ==============================
using System.Text.Json.Serialization;

namespace AdventureGame.Engine.Models.Elements;

public sealed class Item : GameElement
{
    public Item()
    {
        Flags.TryAdd(FlagKeys.IsMovable, true);
    }

    [JsonIgnore]
    public bool IsMovable
    {
        get => Flags.TryGetValue(FlagKeys.IsMovable, out var v) && v;
        set => Flags[FlagKeys.IsMovable] = value;
    }

    public override void OnDeserialized()
    {
        base.OnDeserialized();
        Flags.TryAdd(FlagKeys.IsMovable, true);
    }

    public override void ValidateStatesOrThrow()
    {
        base.ValidateStatesOrThrow();
        Flags.TryAdd(FlagKeys.IsMovable, true);
    }
}
