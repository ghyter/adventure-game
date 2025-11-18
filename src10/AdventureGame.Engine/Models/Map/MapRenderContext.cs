// ==============================
// AdventureGame.Engine/Models/MapRenderContext.cs
// ==============================
namespace AdventureGame.Engine.Models.Map;

/// <summary>
/// Immutable configuration for map rendering.
/// This record is passed as a CascadingParameter so that child components
/// (MapGrid, MapScene, MapExits, etc.) can all use the same consistent context.
/// </summary>
public sealed record MapRenderContext
{
    // ----------------------------
    // Grid dimensions and scaling
    // ----------------------------

    /// <summary>
    /// The width of a single grid cell, in pixels.
    /// </summary>
    public int CellWidth { get; init; } = 100;

    /// <summary>
    /// The height of a single grid cell, in pixels.
    /// </summary>
    public int CellHeight { get; init; } = 100;

    /// <summary>
    /// Current zoom level or scaling factor (1.0 = 100%).
    /// </summary>
    public double Scale { get; init; } = 1.0;

    // ----------------------------
    // Grid coordinate bounds
    // ----------------------------

    /// <summary>
    /// The minimum X coordinate visible on the map grid.
    /// </summary>
    public int MinX { get; init; } = -1;

    /// <summary>
    /// The maximum X coordinate visible on the map grid.
    /// </summary>
    public int MaxX { get; init; } = 1;

    /// <summary>
    /// The minimum Y coordinate visible on the map grid.
    /// </summary>
    public int MinY { get; init; } = -1;

    /// <summary>
    /// The maximum Y coordinate visible on the map grid.
    /// </summary>
    public int MaxY { get; init; } = 1;

    // ----------------------------
    // Rendering preferences
    // ----------------------------

    /// <summary>
    /// Whether the grid lines should be drawn.
    /// </summary>
    public bool ShowGrid { get; init; } = true;

    /// <summary>
    /// Whether coordinate labels should be drawn.
    /// </summary>
    public bool ShowLabels { get; init; } = true;

    /// <summary>
    /// Optional name of the active color palette or theme.
    /// </summary>
    public string Theme { get; init; } = "default";

    // ----------------------------
    // Derived metrics
    // ----------------------------

    /// <summary>
    /// Computed logical map width in pixels.
    /// </summary>
    public double LogicalWidth => (MaxX - MinX + 1) * CellWidth;

    /// <summary>
    /// Computed logical map height in pixels.
    /// </summary>
    public double LogicalHeight => (MaxY - MinY + 1) * CellHeight;

    // ----------------------------
    // Convenience constructors
    // ----------------------------

    /// <summary>
    /// Creates a modified copy of the context with a new scale.
    /// </summary>
    public MapRenderContext WithScale(double newScale) => this with { Scale = newScale };

    /// <summary>
    /// Creates a modified copy of the context with new coordinate bounds.
    /// </summary>
    public MapRenderContext WithBounds(int minX, int maxX, int minY, int maxY)
        => this with { MinX = minX, MaxX = maxX, MinY = minY, MaxY = maxY };

    /// <summary>
    /// Creates a modified copy of the context with different grid cell dimensions.
    /// </summary>
    public MapRenderContext WithCellSize(int width, int height)
        => this with { CellWidth = width, CellHeight = height };
}