using AdventureGame.Engine.Helpers;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Parameters;
using AdventureGame.Engine.Runtime;

namespace AdventureGame.Engine.Conditions.Implementations;

/// <summary>
/// Condition operator that performs a dice check against a threshold.
/// Returns true if the dice roll meets or exceeds the difficulty.
/// </summary>
public sealed class DiceCheckOperator : IConditionOperator
{
    public string Key => "diceCheck";
    
    public string DisplayName => "Dice Check";
    
    public string Description => "Rolls dice and checks if the result meets or exceeds a threshold";
    
    public IReadOnlyList<ParameterDescriptor> Parameters { get; } =
    [
        new()
        {
            Name = "expression",
            DisplayName = "Expression",
            ParameterType = "diceExpression",
            IsOptional = false,
            Description = "Dice expression to roll (e.g., '1d20+5')",
            DefaultValue = "1d20"
        },
        new()
        {
            Name = "threshold",
            DisplayName = "Threshold",
            ParameterType = "number",
            IsOptional = false,
            Description = "Difficulty threshold to meet or exceed"
        }
    ];
    
    public bool Evaluate(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("expression", out var expression) || string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        if (!parameters.TryGetValue("threshold", out var thresholdStr) || !int.TryParse(thresholdStr, out var threshold))
        {
            return false;
        }

        try
        {
            var result = DiceHelperExtensions.RollExpression(expression, threshold);
            
            // Log the roll to the round for transparency
            var success = result.Total >= threshold;
            round.Output.Add($"Dice Check: {expression} = {result.Total} vs DC {threshold} - {(success ? "Success" : "Failed")}");
            
            return success;
        }
        catch (Exception ex)
        {
            round.Output.Add($"Error in dice check: {ex.Message}");
            return false;
        }
    }
}
