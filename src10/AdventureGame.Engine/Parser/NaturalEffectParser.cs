namespace AdventureGame.Engine.Parser;

using AdventureGame.Engine.Effects;
using AdventureGame.Engine.Semantics;
using System.Text.RegularExpressions;

/// <summary>
/// Parses natural language effect statements into AST nodes.
/// Supports: set, increment, decrement, describe, show, print, teleport, add to inventory
/// </summary>
public class NaturalEffectParser
{
    private readonly SemanticPropertyResolver _resolver = new();

    /// <summary>
    /// Parses a natural language effect statement.
    /// </summary>
    public EffectNode? Parse(string statement)
    {
        if (string.IsNullOrWhiteSpace(statement))
            return null;

        var normalized = statement.Trim().ToLowerInvariant();

        // Pattern: set <target> to <semantic>
        // Example: "set target to open"
        var setToMatch = Regex.Match(normalized, @"^set\s+(\w+)\s+to\s+(\w+)$");
        if (setToMatch.Success)
        {
            return new SetSemanticEffect
            {
                Subject = setToMatch.Groups[1].Value,
                SemanticName = setToMatch.Groups[2].Value
            };
        }

        // Pattern: set <target> <property> to <value>
        // Example: "set player health to 10"
        var setPropertyMatch = Regex.Match(normalized, @"^set\s+(\w+)\s+(\w+)\s+to\s+(.+)$");
        if (setPropertyMatch.Success)
        {
            var subject = setPropertyMatch.Groups[1].Value;
            var propertyName = setPropertyMatch.Groups[2].Value;
            var valueStr = setPropertyMatch.Groups[3].Value.Trim();

            // Try parsing as integer (attribute)
            if (int.TryParse(valueStr, out var intValue))
            {
                return new SetAttributeEffect
                {
                    Subject = subject,
                    AttributeName = propertyName,
                    Value = intValue
                };
            }

            // Try parsing as boolean (flag)
            if (bool.TryParse(valueStr, out var boolValue))
            {
                return new SetFlagEffect
                {
                    Subject = subject,
                    FlagName = propertyName,
                    Value = boolValue
                };
            }

            // Otherwise treat as string property
            return new SetPropertyEffect
            {
                Subject = subject,
                PropertyName = propertyName,
                Value = valueStr.Trim('"', '\'')
            };
        }

        // Pattern: increment <target> <key> by <amount>
        // Example: "increment player stamina by 1"
        var incrementMatch = Regex.Match(normalized, @"^increment\s+(\w+)\s+(\w+)\s+by\s+(\d+)$");
        if (incrementMatch.Success)
        {
            return new IncrementEffect
            {
                Subject = incrementMatch.Groups[1].Value,
                Key = incrementMatch.Groups[2].Value,
                Amount = int.Parse(incrementMatch.Groups[3].Value)
            };
        }

        // Pattern: decrement <target> <key> by <amount>
        // Example: "decrement target battery by 5"
        var decrementMatch = Regex.Match(normalized, @"^decrement\s+(\w+)\s+(\w+)\s+by\s+(\d+)$");
        if (decrementMatch.Success)
        {
            return new DecrementEffect
            {
                Subject = decrementMatch.Groups[1].Value,
                Key = decrementMatch.Groups[2].Value,
                Amount = int.Parse(decrementMatch.Groups[3].Value)
            };
        }

        // Pattern: describe <target> | show <target>
        // Example: "describe currentScene"
        var describeMatch = Regex.Match(normalized, @"^(describe|show)\s+(\w+)$");
        if (describeMatch.Success)
        {
            return new DescribeEffect
            {
                Subject = describeMatch.Groups[2].Value
            };
        }

        // Pattern: print "<message>"
        // Example: "print 'The door creaks open'"
        var printMatch = Regex.Match(statement, @"^print\s+[""'](.+)[""']$", RegexOptions.IgnoreCase);
        if (printMatch.Success)
        {
            return new PrintEffect
            {
                Subject = "",
                Message = printMatch.Groups[1].Value
            };
        }

        // Pattern: teleport <target>
        // Example: "teleport target"
        var teleportMatch = Regex.Match(normalized, @"^teleport\s+(\w+)$");
        if (teleportMatch.Success)
        {
            return new TeleportEffect
            {
                Subject = teleportMatch.Groups[1].Value
            };
        }

        // Pattern: add <target> to inventory
        // Example: "add target to inventory"
        var addInventoryMatch = Regex.Match(normalized, @"^add\s+(\w+)\s+to\s+inventory$");
        if (addInventoryMatch.Success)
        {
            return new AddToInventoryEffect
            {
                Subject = addInventoryMatch.Groups[1].Value
            };
        }

        // Pattern: remove <target> from inventory
        // Example: "remove target from inventory"
        var removeInventoryMatch = Regex.Match(normalized, @"^remove\s+(\w+)\s+from\s+inventory$");
        if (removeInventoryMatch.Success)
        {
            return new RemoveFromInventoryEffect
            {
                Subject = removeInventoryMatch.Groups[1].Value
            };
        }

        // No match found
        return null;
    }

    /// <summary>
    /// Parses multiple effect statements (one per line).
    /// </summary>
    public List<EffectNode> ParseMultiple(string statements)
    {
        if (string.IsNullOrWhiteSpace(statements))
            return new List<EffectNode>();

        var lines = statements.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var effects = new List<EffectNode>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("//") || trimmed.StartsWith("#"))
                continue;

            var effect = Parse(trimmed);
            if (effect != null)
            {
                effects.Add(effect);
            }
        }

        return effects;
    }

    /// <summary>
    /// Validates that an effect statement can be parsed.
    /// </summary>
    public (bool success, string? error) Validate(string statement)
    {
        if (string.IsNullOrWhiteSpace(statement))
            return (false, "Statement is empty");

        var effect = Parse(statement);
        if (effect == null)
            return (false, "Could not parse effect statement. Check syntax.");

        return (true, null);
    }
}
