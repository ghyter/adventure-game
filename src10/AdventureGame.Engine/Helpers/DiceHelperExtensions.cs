using System.Text.RegularExpressions;

namespace AdventureGame.Engine.Helpers;

/// <summary>
/// Extensions to DiceHelper for complex dice expressions.
/// Supports multi-group expressions like "2d6+1d4+3" or "1d20-2".
/// </summary>
public static class DiceHelperExtensions
{
    // Pattern to match dice expressions with modifiers: "2d6+1d4+3", "1d20-2", etc.
    private static readonly Regex ComplexDiceRegex = new(
        @"([+-]?\s*\d*d\d+[ad]?|[+-]?\s*\d+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Rolls a complex dice expression and returns detailed results.
    /// Supports expressions like "2d6+1d4+3", "1d20-2+1d6", etc.
    /// </summary>
    /// <param name="expression">The dice expression to evaluate</param>
    /// <param name="threshold">Optional difficulty threshold for success/failure</param>
    /// <returns>Detailed roll result with breakdown</returns>
    public static RollResult RollExpression(string expression, int threshold = 0)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentException("Dice expression cannot be empty", nameof(expression));
        }

        var normalized = expression.Replace(" ", "");
        var matches = ComplexDiceRegex.Matches(normalized);
        
        if (matches.Count == 0)
        {
            throw new ArgumentException($"Invalid dice expression: {expression}");
        }

        int totalRoll = 0;
        int fixedBonus = 0;
        var rolls = new List<(string expr, int value)>();

        foreach (Match match in matches)
        {
            var token = match.Value.Trim();
            
            // Check if it's a dice roll (contains 'd')
            if (token.Contains('d', StringComparison.OrdinalIgnoreCase))
            {
                // Extract sign
                int sign = 1;
                if (token.StartsWith('-'))
                {
                    sign = -1;
                    token = token.Substring(1);
                }
                else if (token.StartsWith('+'))
                {
                    token = token.Substring(1);
                }

                try
                {
                    int rollValue = DiceHelper.Roll(token);
                    totalRoll += sign * rollValue;
                    rolls.Add((token, sign * rollValue));
                }
                catch (ArgumentException)
                {
                    throw new ArgumentException($"Invalid dice notation in expression: {token}");
                }
            }
            else
            {
                // It's a fixed modifier
                if (int.TryParse(token, out int modifier))
                {
                    fixedBonus += modifier;
                }
            }
        }

        int total = totalRoll + fixedBonus;

        return new RollResult
        {
            DiceExpression = expression,
            Roll = totalRoll,
            Bonus = fixedBonus,
            Total = total,
            Threshold = threshold
        };
    }

    /// <summary>
    /// Checks if a dice expression meets or exceeds a threshold.
    /// </summary>
    /// <param name="expression">The dice expression to roll</param>
    /// <param name="threshold">The target difficulty</param>
    /// <returns>True if the roll meets or exceeds the threshold</returns>
    public static bool CheckSuccess(string expression, int threshold)
    {
        var result = RollExpression(expression, threshold);
        return result.Total >= threshold;
    }

    /// <summary>
    /// Rolls a percentage check (1d100).
    /// </summary>
    /// <param name="percentage">The percentage chance of success (0-100)</param>
    /// <returns>True if the roll is less than or equal to the percentage</returns>
    public static bool PercentageCheck(int percentage)
    {
        if (percentage < 0 || percentage > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be between 0 and 100");
        }

        int roll = DiceHelper.Roll("1d100");
        return roll <= percentage;
    }

    /// <summary>
    /// Validates if a string is a valid dice expression.
    /// </summary>
    /// <param name="expression">The expression to validate</param>
    /// <returns>True if the expression can be parsed and rolled</returns>
    public static bool IsValidExpression(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return false;

        try
        {
            RollExpression(expression);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
