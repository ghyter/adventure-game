#nullable enable
using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Elements;
using ExitPairModel = AdventureGame.Engine.Models.ExitPair;

namespace AdventureGame.Engine.Models.Map;

public enum SelectionKind
{
    None,
    GridSquare,
    Scene,
    ExitPair,
    Item,
    Npc,
    Player
}

public sealed class MapSelection
{
    public SelectionKind Kind { get; }
    /// <summary>Holds the selected GameElement (Scene/Item/Npc/Player) when applicable.</summary>
    public GameElement? Element { get; }
    /// <summary>Holds the selected grid coordinate when Kind == GridSquare.</summary>
    public (int X, int Y)? GridCell { get; }
    /// <summary>Holds the selected ExitPair when Kind == ExitPair.</summary>
    public ExitPairModel? Pair { get; }

    private MapSelection(
        SelectionKind kind,
        GameElement? element = null,
        (int X, int Y)? gridCell = null,
        ExitPairModel? pair = null)
    {
        Kind = kind;
        Element = element;
        GridCell = gridCell;
        Pair = pair;
    }

    public static MapSelection None() => new(SelectionKind.None);

    public static MapSelection FromGridSquare(int x, int y)
        => new(SelectionKind.GridSquare, gridCell: (x, y));

    public static MapSelection FromScene(Scene scene)
        => new(SelectionKind.Scene, element: scene);

    public static MapSelection FromExitPair(Exit a, Exit b)
        => new(SelectionKind.ExitPair, pair: new ExitPairModel(a, b));

    public static MapSelection FromItem(Item item)
        => new(SelectionKind.Item, element: item);

    public static MapSelection FromNpc(Npc npc)
        => new(SelectionKind.Npc, element: npc);

    public static MapSelection FromPlayer(Player player)
        => new(SelectionKind.Player, element: player);

    // Optional helpers (consistent names)
    public bool TryGetGrid(out (int X, int Y) grid) { grid = GridCell ?? default; return GridCell.HasValue; }
    public bool TryGetElement<T>(out T? result) where T : GameElement { result = Element as T; return result is not null; }
    public bool TryGetExitPair(out ExitPairModel? pair) { pair = Pair; return pair is not null; }
}
