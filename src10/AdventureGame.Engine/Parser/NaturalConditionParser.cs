namespace AdventureGame.Engine.Parser;

using AdventureGame.Engine.Conditions;
using System.Text.RegularExpressions;

/// <summary>
/// Parses natural language condition statements into AST nodes.
/// Supports: is <type>, is <state>, has <item>, <comparison> <number>
/// </summary>
public class NaturalConditionParser
{
    private static readonly HashSet<string> KnownTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "exit", "scene", "npc", "item", "player", "level"
    };

    /// <summary>
    /// Parses a natural language condition statement.
    /// </summary>
    public ConditionNode? Parse(string statement)
    {
        if (string.IsNullOrWhiteSpace(statement))
            return null;

        var normalized = statement.Trim().ToLowerInvariant();

        // Remove leading "when" if present
        if (normalized.StartsWith("when "))
        {
            normalized = normalized.Substring(5).Trim();
        }

        // Pattern: <subject> is <type>
        // Example: "target is exit"
        var isTypeMatch = Regex.Match(normalized, @"^(\w+)\s+is\s+(\w+)$");
        if (isTypeMatch.Success)
        {
            var subject = isTypeMatch.Groups[1].Value;
            var typeOrState = isTypeMatch.Groups[2].Value;

            // Check if it's a known element type
            if (KnownTypes.Contains(typeOrState))
            {
                return new KindCheckCondition
                {
                    Subject = subject,
                    Kind = typeOrState
                };
            }

            // Otherwise treat as state check
            return new StateCheckCondition
            {
                Subject = subject,
                StateName = typeOrState
            };
        }

        // Pattern: <subject> has <item>
        // Example: "player has key"
        var hasMatch = Regex.Match(normalized, @"^(\w+)\s+has\s+(\w+)$");
        if (hasMatch.Success)
        {
            return new HasItemCondition
            {
                Subject = hasMatch.Groups[1].Value,
                ItemName = hasMatch.Groups[2].Value
            };
        }

        // Pattern: <subject> <property> <comparison> <value>
        // Example: "player health > 5"
        var comparisonMatch = Regex.Match(normalized, @"^(\w+)\s+(\w+)\s*([><=!]+)\s*(\d+)$");
        if (comparisonMatch.Success)
        {
            var subject = comparisonMatch.Groups[1].Value;
            var key = comparisonMatch.Groups[2].Value;
            var op = comparisonMatch.Groups[3].Value;
            var value = int.Parse(comparisonMatch.Groups[4].Value);

            var compOp = op switch
            {
                ">" => ComparisonOperator.GreaterThan,
                "<" => ComparisonOperator.LessThan,
                ">=" => ComparisonOperator.GreaterThanOrEqual,
                "<=" => ComparisonOperator.LessThanOrEqual,
                "==" or "=" => ComparisonOperator.Equal,
                "!=" => ComparisonOperator.NotEqual,
                _ => ComparisonOperator.Equal
            };

            return new ComparisonCondition
            {
                Subject = subject,
                Key = key,
                Operator = compOp,
                Value = value
            };
        }

        // Pattern: <subject> <property> is <value>
        // Example: "player health is 10"
        var propertyIsMatch = Regex.Match(normalized, @"^(\w+)\s+(\w+)\s+is\s+(.+)$");
        if (propertyIsMatch.Success)
        {
            var subject = propertyIsMatch.Groups[1].Value;
            var propertyName = propertyIsMatch.Groups[2].Value;
            var valueStr = propertyIsMatch.Groups[3].Value.Trim();

            // Try parsing as integer (attribute)
            if (int.TryParse(valueStr, out var intValue))
            {
                return new AttributeCheckCondition
                {
                    Subject = subject,
                    AttributeName = propertyName,
                    ExpectedValue = intValue
                };
            }

            // Try parsing as boolean (flag)
            if (bool.TryParse(valueStr, out var boolValue))
            {
                return new FlagCheckCondition
                {
                    Subject = subject,
                    FlagName = propertyName,
                    ExpectedValue = boolValue
                };
            }

            // Otherwise treat as string property
            return new PropertyCheckCondition
            {
                Subject = subject,
                PropertyName = propertyName,
                ExpectedValue = valueStr.Trim('"', '\'')
            };
        }

        // No match found
        return null;
    }

    /// <summary>
    /// Parses multiple condition statements with AND/OR logic.
    /// </summary>
    public ConditionNode? ParseMultiple(string statements)
    {
        if (string.IsNullOrWhiteSpace(statements))
            return null;

        // Split by "and" or "or"
        var andParts = statements.ToLowerInvariant().Split(new[] { " and " }, StringSplitOptions.RemoveEmptyEntries);
        
        if (andParts.Length > 1)
        {
            // Multiple conditions joined with AND
            ConditionNode? current = null;
            foreach (var part in andParts)
            {
                var condition = Parse(part.Trim());
                if (condition != null)
                {
                    if (current == null)
                    {
                        current = condition;
                    }
                    else
                    {
                        current = new LogicalAnd { Left = current, Right = condition };
                    }
                }
            }
            return current;
        }

        var orParts = statements.ToLowerInvariant().Split(new[] { " or " }, StringSplitOptions.RemoveEmptyEntries);
        
        if (orParts.Length > 1)
        {
            // Multiple conditions joined with OR
            ConditionNode? current = null;
            foreach (var part in orParts)
            {
                var condition = Parse(part.Trim());
                if (condition != null)
                {
                    if (current == null)
                    {
                        current = condition;
                    }
                    else
                    {
                        current = new LogicalOr { Left = current, Right = condition };
                    }
                }
            }
            return current;
        }

        // Single condition
        return Parse(statements);
    }

    /// <summary>
    /// Validates that a condition statement can be parsed.
    /// </summary>
    public (bool success, string? error) Validate(string statement)
    {
        if (string.IsNullOrWhiteSpace(statement))
            return (false, "Statement is empty");

        var condition = Parse(statement);
        if (condition == null)
            return (false, "Could not parse condition statement. Check syntax.");

        return (true, null);
    }
}
