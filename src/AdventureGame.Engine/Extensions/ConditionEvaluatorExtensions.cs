using AdventureGame.Engine.Models;
using AdventureGame.Engine.Runtime;

namespace AdventureGame.Engine.Extensions;

public static class ConditionEvaluatorExtensions
{

    public static bool Evaluate(this Condition condition, GameSession session)
    {
        // Find the referenced element
        var element = session.FindByNameOrAlias(condition.GameElementId);
        if (element is null) return false;

        // Example basic rules
        return condition.Rule switch
        {
            "HasState" => string.Equals(element.DefaultState, condition.Value, StringComparison.OrdinalIgnoreCase),
            "IsVisible" => element.IsVisible == bool.Parse(condition.Value),
            "HasProperty" => element.Properties.TryGetValue(condition.Comparison, out var v)
                             && string.Equals(v, condition.Value, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }
}
