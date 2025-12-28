using AdventureGame.Engine.Models.Round;
using AdventureGame.Engine.Runtime;
using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Actions;

namespace AdventureGame.Engine.Extensions;

public static class ConditionEvaluatorExtensions
{

    public static bool Evaluate(this Condition condition, GameSession session)
    {
        /*  // Find the referenced element
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
          };*/
        return false;
    }

    /// <summary>
    /// Evaluate a single legacy Condition against a provided element scope.
    /// This is a placeholder; implement property-path resolution in future.
    /// </summary>
    public static bool Evaluate(this Condition condition, GameSession session, IEnumerable<GameElement> scope)
    {
        return condition.Evaluate(session);
    }

    /// <summary>
    /// Evaluate a structured ConditionGroup with And/Or semantics over a scope.
    /// Placeholder implementation returns false for empty groups and combines child results.
    /// </summary>
    public static bool Evaluate(this ConditionGroup group, GameSession session, IEnumerable<GameElement> scope)
    {
        if (group is null || group.Nodes.Count == 0) return false;
        bool Seed(bool first) => group.Operator == LogicOperator.And ? true : false;
        bool result = Seed(true);
        foreach (var node in group.Nodes)
        {
            bool nodeResult = node.Condition is not null
                ? node.Condition.Evaluate(session, scope)
                : (node.ConditionGroup is not null && node.ConditionGroup.Evaluate(session, scope));

            if (group.Operator == LogicOperator.And)
            {
                result &= nodeResult;
                if (!result) return false; // short-circuit
            }
            else
            {
                result |= nodeResult;
                if (result) return true; // short-circuit
            }
        }
        return result;
    }
}
