using AdventureGame.Engine.Helpers;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Runtime;

namespace AdventureGame.Engine.Actions.Implementations;

/// <summary>
/// Effect action that rolls dice and logs the result.
/// Supports complex dice expressions like "2d6+3" or "1d20+1d4".
/// </summary>
public sealed class RollDiceEffect : IEffectAction
{
    public string Key => "rollDice";
    
    public string DisplayName => "Roll Dice";
    
    public string Description => "Rolls dice using the specified expression and logs the result";
    
    public IReadOnlyList<EffectParameterDescriptor> Parameters { get; } = new List<EffectParameterDescriptor>
    {
        new()
        {
            Name = "expression",
            Kind = EffectParameterKind.DiceExpression,
            IsRequired = true,
            Description = "Dice expression (e.g., '2d6+3', '1d20')",
            DefaultValue = "1d20"
        },
        new()
        {
            Name = "label",
            Kind = EffectParameterKind.Value,
            IsRequired = false,
            Description = "Optional label for the roll"
        }
    };
    
    public Task ExecuteAsync(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("expression", out var expression) || string.IsNullOrWhiteSpace(expression))
        {
            round.Output.Add("Error: No dice expression provided");
            return Task.CompletedTask;
        }

        try
        {
            var result = DiceHelperExtensions.RollExpression(expression);
            
            var label = parameters.TryGetValue("label", out var lbl) && !string.IsNullOrWhiteSpace(lbl) 
                ? $"{lbl}: " 
                : "";
            
            round.Output.Add($"{label}Rolled {expression} = {result.Total} (dice: {result.Roll}, bonus: {result.Bonus})");
            
            // Store result in round for potential use by other effects
            round.SetValue("lastRollTotal", result.Total.ToString());
        }
        catch (Exception ex)
        {
            round.Output.Add($"Error rolling dice: {ex.Message}");
        }
        
        return Task.CompletedTask;
    }
}
