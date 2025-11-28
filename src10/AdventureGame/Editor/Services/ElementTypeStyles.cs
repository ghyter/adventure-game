using Radzen;

namespace AdventureGame.Editor.Services;

/// <summary>
/// Centralized configuration for element type visual styling (colors, icons, etc.)
/// Used across the application for consistent element type representation.
/// Colors match the map visualization for consistency.
/// </summary>
public static class ElementTypeStyles
{
    /// <summary>
    /// Converts a GameElement.Kind string to an ElementKind enum value.
    /// </summary>
    public static ElementKind ParseKind(string kindString)
        => kindString?.ToLowerInvariant() switch
        {
            "scene" => ElementKind.Scene,
            "player" => ElementKind.Player,
            "npc" => ElementKind.Npc,
            "item" => ElementKind.Item,
            "exit" => ElementKind.Exit,
            "level" => ElementKind.Level,
            _ => ElementKind.Item  // Default
        };

    /// <summary>
    /// Gets the badge style for a specific element kind string.
    /// </summary>
    public static BadgeStyle GetBadgeStyle(string kindString)
        => GetBadgeStyle(ParseKind(kindString));

    /// <summary>
    /// Gets the badge style for a specific element kind.
    /// Uses colors that match the map visualization.
    /// </summary>
    public static BadgeStyle GetBadgeStyle(ElementKind kind)
        => kind switch
        {
            ElementKind.Scene => BadgeStyle.Warning,      // Tan/Beige (matches map #f5e6d3)
            ElementKind.Player => BadgeStyle.Success,     // Green/Mint (matches map #6ee7b7)
            ElementKind.Npc => BadgeStyle.Danger,         // Pink/Rose (matches map #fb7185)
            ElementKind.Item => BadgeStyle.Info,          // Light Blue (matches map #93c5fd)
            ElementKind.Exit => BadgeStyle.Secondary,     // Orange (matches map #f97316)
            ElementKind.Level => BadgeStyle.Dark,         // Dark Gray
            _ => BadgeStyle.Light                         // Default: Light Gray
        };

    /// <summary>
    /// Gets the CSS color value for a specific element kind string.
    /// </summary>
    public static string GetColor(string kindString)
        => GetColor(ParseKind(kindString));

    /// <summary>
    /// Gets the CSS color value for a specific element kind.
    /// These colors match the map visualization for consistency.
    /// </summary>
    public static string GetColor(ElementKind kind)
        => kind switch
        {
            ElementKind.Scene => "#f5e6d3",      // Map Scene fill color (tan/beige)
            ElementKind.Player => "#6ee7b7",     // Map Player fill color (mint green)
            ElementKind.Npc => "#fb7185",        // Map NPC fill color (pink/rose)
            ElementKind.Item => "#93c5fd",       // Map Item fill color (light blue)
            ElementKind.Exit => "#f97316",       // Map Exit fill color (orange)
            ElementKind.Level => "#1f2937",      // Dark gray for levels
            _ => "#f8f9fa"                       // Light gray default
        };

    /// <summary>
    /// Gets a subtle, very light background color for tiles and cards.
    /// </summary>
    public static string GetBackgroundColor(string kindString)
        => GetBackgroundColor(ParseKind(kindString));

    /// <summary>
    /// Gets a subtle, very light background color for tiles and cards.
    /// These are much lighter versions of the main colors for subtle visual distinction.
    /// </summary>
    public static string GetBackgroundColor(ElementKind kind)
        => kind switch
        {
            ElementKind.Scene => "#fdfaf6",      // Very light tan/beige
            ElementKind.Player => "#f0fdf9",     // Very light mint green
            ElementKind.Npc => "#fef2f3",        // Very light pink/rose
            ElementKind.Item => "#f0f9ff",       // Very light blue
            ElementKind.Exit => "#fff7ed",       // Very light orange
            ElementKind.Level => "#f9fafb",      // Very light gray
            _ => "#ffffff"                       // White default
        };

    /// <summary>
    /// Gets the stroke/border color for a specific element kind string.
    /// </summary>
    public static string GetStrokeColor(string kindString)
        => GetStrokeColor(ParseKind(kindString));

    /// <summary>
    /// Gets the stroke/border color for a specific element kind.
    /// These match the map's stroke colors.
    /// </summary>
    public static string GetStrokeColor(ElementKind kind)
        => kind switch
        {
            ElementKind.Scene => "#b89f85",      // Map Scene stroke color
            ElementKind.Player => "#10b981",     // Darker green
            ElementKind.Npc => "#f43f5e",        // Darker rose
            ElementKind.Item => "#3b82f6",       // Darker blue
            ElementKind.Exit => "#b45309",       // Map Exit stroke color (dark orange)
            ElementKind.Level => "#111827",      // Very dark gray
            _ => "#6c757d"                       // Gray default
        };

    /// <summary>
    /// Gets the Material Icon name for a specific element kind string.
    /// </summary>
    public static string GetIcon(string kindString)
        => GetIcon(ParseKind(kindString));

    /// <summary>
    /// Gets the Material Icon name for a specific element kind.
    /// </summary>
    public static string GetIcon(ElementKind kind)
        => kind switch
        {
            ElementKind.Scene => "location_city",
            ElementKind.Player => "person",
            ElementKind.Npc => "group",
            ElementKind.Item => "inventory_2",
            ElementKind.Exit => "door_front",
            ElementKind.Level => "layers",
            _ => "help_outline"
        };

    /// <summary>
    /// Gets a descriptive label for the element kind string.
    /// </summary>
    public static string GetLabel(string kindString)
        => GetLabel(ParseKind(kindString));

    /// <summary>
    /// Gets a descriptive label for the element kind.
    /// </summary>
    public static string GetLabel(ElementKind kind)
        => kind switch
        {
            ElementKind.Scene => "Scene",
            ElementKind.Player => "Player",
            ElementKind.Npc => "NPC",
            ElementKind.Item => "Item",
            ElementKind.Exit => "Exit",
            ElementKind.Level => "Level",
            _ => "Unknown"
        };
}
