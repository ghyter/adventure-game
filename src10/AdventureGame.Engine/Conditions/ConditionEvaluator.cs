using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Runtime;

namespace AdventureGame.Engine.Conditions;

/// <summary>
/// Evaluates condition blocks with nested AND/OR/NOT logic.
/// Handles recursive evaluation of complex condition trees.
/// </summary>
public sealed class ConditionEvaluator
{
    private readonly IConditionOperatorCatalog _catalog;

    public ConditionEvaluator(IConditionOperatorCatalog catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    /// <summary>
    /// Evaluates a condition block against the current game state.
    /// </summary>
    /// <param name="block">The condition block to evaluate</param>
    /// <param name="round">The current game round</param>
    /// <param name="session">The game session</param>
    /// <returns>True if all conditions in the block are satisfied, false otherwise</returns>
    public bool Evaluate(ConditionBlock block, GameRound round, GameSession session)
    {
        if (block == null) return true;

        // Evaluate AllOf (AND logic) - all must be true
        if (block.AllOf.Any())
        {
            if (!block.AllOf.All(cond => EvaluateCondition(cond, round, session)))
                return false;
        }

        // Evaluate AnyOf (OR logic) - at least one must be true
        if (block.AnyOf.Any())
        {
            if (!block.AnyOf.Any(cond => EvaluateCondition(cond, round, session)))
                return false;
        }

        // Evaluate NoneOf (NOT logic) - none must be true
        if (block.NoneOf.Any())
        {
            if (block.NoneOf.Any(cond => EvaluateCondition(cond, round, session)))
                return false;
        }

        // Evaluate nested blocks recursively
        if (block.NestedBlocks.Any())
        {
            if (!block.NestedBlocks.All(nested => Evaluate(nested, round, session)))
                return false;
        }

        return true;
    }

    private bool EvaluateCondition(ConditionDefinition definition, GameRound round, GameSession session)
    {
        var op = _catalog.GetByKey(definition.OperatorKey);
        if (op == null)
        {
            // Log warning: unknown operator
            return false;
        }

        try
        {
            return op.Evaluate(round, session, definition.Parameters);
        }
        catch (Exception)
        {
            // Log error: condition evaluation failed
            return false;
        }
    }
}
