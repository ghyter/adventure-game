using System.Text.Json.Serialization;

namespace AdventureGame.Engine.Models.Actions;

/// <summary>
/// Represents a pattern-based target definition using breadcrumb syntax.
/// Examples:
/// - type/* (any game element type)
/// - type/npc/* (any npc)
/// - type/npc/Mrs. White (specific npc)
/// - tags/weapons (any element with "weapons" tag)
/// </summary>
public sealed class TargetPattern
{
    /// <summary>
    /// The pattern category: "type" or "tags"
    /// </summary>
    [JsonInclude]
    public string Category { get; set; } = "type";

    /// <summary>
    /// The specific type when Category is "type" (e.g., "item", "npc", "scene", "exit", "player", "level")
    /// Null or "*" means all types
    /// </summary>
    [JsonInclude]
    public string? TypeFilter { get; set; }

    /// <summary>
    /// The tag name when Category is "tags"
    /// </summary>
    [JsonInclude]
    public string? TagFilter { get; set; }

    /// <summary>
    /// Specific element name or ID. Null or "*" means any element matching the other filters
    /// </summary>
    [JsonInclude]
    public string? NameFilter { get; set; }

    /// <summary>
    /// Returns the breadcrumb display format
    /// </summary>
    public string ToBreadcrumb()
    {
        if (Category == "tags")
        {
            return $"tags/{TagFilter ?? "*"}";
        }

        // Type category
        var parts = new List<string> { "type" };
        
        if (!string.IsNullOrEmpty(TypeFilter) && TypeFilter != "*")
        {
            parts.Add(TypeFilter);
            
            if (!string.IsNullOrEmpty(NameFilter) && NameFilter != "*")
            {
                parts.Add(NameFilter);
            }
            else
            {
                parts.Add("*");
            }
        }
        else
        {
            parts.Add("*");
        }

        return string.Join("/", parts);
    }

    /// <summary>
    /// Checks if this pattern covers another pattern (making the other redundant)
    /// </summary>
    public bool Covers(TargetPattern other)
    {
        // Different categories can coexist
        if (Category != other.Category)
            return false;

        if (Category == "tags")
        {
            // tags/weapons covers tags/weapons exactly
            return TagFilter == other.TagFilter;
        }

        // Type category comparison
        
        // type/* covers everything
        if (string.IsNullOrEmpty(TypeFilter) || TypeFilter == "*")
            return true;

        // type/npc/* doesn't cover type/item/*
        if (TypeFilter != other.TypeFilter)
            return false;

        // type/npc/* covers type/npc/Mrs. White
        if (string.IsNullOrEmpty(NameFilter) || NameFilter == "*")
            return true;

        // type/npc/Mrs. White only covers type/npc/Mrs. White
        return NameFilter == other.NameFilter;
    }

    /// <summary>
    /// Checks if this pattern matches a specific game element
    /// </summary>
    public bool Matches(GameElement element)
    {
        if (Category == "tags")
        {
            return !string.IsNullOrEmpty(TagFilter) && 
                   element.Tags.Contains(TagFilter, StringComparer.OrdinalIgnoreCase);
        }

        // Type category matching
        
        // Check type filter
        if (!string.IsNullOrEmpty(TypeFilter) && TypeFilter != "*")
        {
            if (!element.Kind.Equals(TypeFilter, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // Check name filter
        if (!string.IsNullOrEmpty(NameFilter) && NameFilter != "*")
        {
            return element.Name.Equals(NameFilter, StringComparison.OrdinalIgnoreCase) ||
                   element.Aliases.Contains(NameFilter, StringComparer.OrdinalIgnoreCase);
        }

        return true;
    }

    public override string ToString() => ToBreadcrumb();
}
