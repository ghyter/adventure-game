using AdventureGame.Engine.Helpers;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Parameters;
using AdventureGame.Engine.Runtime;

namespace AdventureGame.Engine.Conditions.Implementations;

/// <summary>
/// Condition operator that performs a percentage-based random check.
/// Rolls 1d100 and checks if it's less than or equal to the percentage.
/// </summary>
public sealed class PercentageChanceOperator : IConditionOperator
{
    public string Key => "percentageChance";
    
    public string DisplayName => "Percentage Chance";
    
    public string Description => "Performs a percentage-based random check (1-100)";
    
    public IReadOnlyList<ParameterDescriptor> Parameters { get; } =
    [
        new()
        {
            Name = "percentage",
            DisplayName = "Percentage",
            ParameterType = "number",
            IsOptional = false,
            Description = "Percentage chance of success (0-100)"
        }
    ];
    
    public bool Evaluate(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("percentage", out var percentageStr) || !int.TryParse(percentageStr, out var percentage))
        {
            return false;
        }

        if (percentage < 0 || percentage > 100)
        {
            round.Output.Add($"Error: Percentage must be between 0 and 100, got {percentage}");
            return false;
        }

        try
        {
            bool success = DiceHelperExtensions.PercentageCheck(percentage);
            round.Output.Add($"Percentage Check: {percentage}% chance - {(success ? "Success" : "Failed")}");
            return success;
        }
        catch (Exception ex)
        {
            round.Output.Add($"Error in percentage check: {ex.Message}");
            return false;
        }
    }
}
