using AdventureGame.Engine.Models.Actions;

namespace AdventureGame.Engine.Services;

/// <summary>
/// Service for validating target patterns and checking for redundancies
/// </summary>
public sealed class TargetPatternService
{
    /// <summary>
    /// Checks if adding a new pattern would be redundant given existing patterns
    /// </summary>
    /// <param name="newPattern">The pattern to add</param>
    /// <param name="existingPatterns">Current patterns</param>
    /// <returns>True if the pattern would be redundant, false otherwise</returns>
    public bool IsRedundant(TargetPattern newPattern, IEnumerable<TargetPattern> existingPatterns)
    {
        foreach (var existing in existingPatterns)
        {
            if (existing.Covers(newPattern))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets a list of patterns that would become redundant if a new pattern is added
    /// </summary>
    /// <param name="newPattern">The pattern to add</param>
    /// <param name="existingPatterns">Current patterns</param>
    /// <returns>List of patterns that would be covered by the new pattern</returns>
    public List<TargetPattern> GetCoveredPatterns(TargetPattern newPattern, IEnumerable<TargetPattern> existingPatterns)
    {
        return existingPatterns.Where(existing => newPattern.Covers(existing)).ToList();
    }

    /// <summary>
    /// Validates that a pattern is well-formed
    /// </summary>
    public (bool IsValid, string? ErrorMessage) ValidatePattern(TargetPattern pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern.Category))
        {
            return (false, "Category is required");
        }

        if (pattern.Category != "type" && pattern.Category != "tags")
        {
            return (false, "Category must be 'type' or 'tags'");
        }

        if (pattern.Category == "tags")
        {
            if (string.IsNullOrWhiteSpace(pattern.TagFilter))
            {
                return (false, "Tag filter is required for tags category");
            }
        }
        else
        {
            // Type category - TypeFilter can be null or * for "all types"
            // NameFilter can be null or * for "all names"
        }

        return (true, null);
    }

    /// <summary>
    /// Parses a breadcrumb string into a TargetPattern
    /// </summary>
    public (TargetPattern? Pattern, string? ErrorMessage) ParseBreadcrumb(string breadcrumb)
    {
        if (string.IsNullOrWhiteSpace(breadcrumb))
        {
            return (null, "Breadcrumb cannot be empty");
        }

        var parts = breadcrumb.Split('/');
        
        if (parts.Length == 0)
        {
            return (null, "Invalid breadcrumb format");
        }

        var category = parts[0].ToLowerInvariant();

        if (category == "tags")
        {
            if (parts.Length != 2)
            {
                return (null, "Tags breadcrumb must be in format: tags/{tagname}");
            }

            return (new TargetPattern
            {
                Category = "tags",
                TagFilter = parts[1]
            }, null);
        }

        if (category == "type")
        {
            var pattern = new TargetPattern { Category = "type" };

            if (parts.Length >= 2)
            {
                pattern.TypeFilter = parts[1] == "*" ? null : parts[1];
            }

            if (parts.Length >= 3)
            {
                pattern.NameFilter = parts[2] == "*" ? null : parts[2];
            }

            return (pattern, null);
        }

        return (null, $"Unknown category: {category}");
    }

    /// <summary>
    /// Gets available element types for UI dropdowns
    /// </summary>
    public List<string> GetAvailableTypes()
    {
        return new List<string> { "item", "npc", "scene", "exit", "player", "level" };
    }
}
