using AdventureGame.Engine.Models.Round;
using AdventureGame.Engine.Runtime;
using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.DSL.Evaluation;
using AdventureGame.Engine.Models.Elements;

namespace AdventureGame.Engine.Extensions;

public static class ConditionEvaluatorExtensions
{
    /// <summary>
    /// Evaluates a GameCondition using the DSL service.
    /// Uses the new ConditionText field for natural language DSL evaluation.
    /// </summary>
    public static bool Evaluate(this GameCondition condition, GameSession session)
    {
        if (condition == null || session == null) return false;

        // Use the new ConditionText field
        if (!string.IsNullOrWhiteSpace(condition.ConditionText))
        {
            // Get or create DSL service from session
            var dslService = session.DslService;
            if (dslService != null)
            {
                // Create evaluation context from session
                var context = new GameSessionDslEvaluationContext(session);
                return dslService.EvaluateText(condition.ConditionText, context);
            }
        }

        return false;
    }

    /// <summary>
    /// Evaluate a single condition against a provided element scope.
    /// </summary>
    public static bool Evaluate(this GameCondition condition, GameSession session, IEnumerable<GameElement> scope)
    {
        return condition.Evaluate(session);
    }

    /// <summary>
    /// Evaluate a structured ConditionGroup with And/Or semantics over a scope.
    /// </summary>
    public static bool Evaluate(this ConditionGroup group, GameSession session, IEnumerable<GameElement> scope)
    {
        if (group is null || group.Nodes.Count == 0) return false;
        
        bool result = group.Operator == LogicOperator.And;
        
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

/// <summary>
/// Implementation of DslEvaluationContext for GameSession.
/// Provides access to game state for DSL condition evaluation.
/// </summary>
internal class GameSessionDslEvaluationContext(GameSession session) : DslEvaluationContext
{
    private readonly GameSession _session = session ?? throw new ArgumentNullException(nameof(session));

    public override object? GetPlayer() => _session.Player;
    
    public override object? GetTarget() => _session.CurrentTarget;
    
    public override object? GetTarget2() => null; // Not implemented in current session
    
    public override object? GetCurrentScene() => _session.CurrentScene;
    
    public override object? GetSession() => _session;
    
    public override object? GetLog() => null; // Not implemented in current session
    
    public override object? GetElement(string kind, string? id)
    {
        if (string.IsNullOrWhiteSpace(id) || _session?.Pack?.Elements == null) return null;

        return _session.Pack.Elements.FirstOrDefault(e => 
            e.Kind == kind && (e.Id.ToString() == id || e.Name == id));
    }
    
    public override int GetVisitCount(string subjectKind, string sceneName)
    {
        // TODO: Implement visit tracking if needed
        return 0;
    }
    
    public override int GetDistance(AdventureGame.Engine.DSL.AST.SubjectRef from, AdventureGame.Engine.DSL.AST.SubjectRef to)
    {
        // TODO: Implement distance calculation
        return 0;
    }
}
