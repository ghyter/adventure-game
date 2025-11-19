namespace AdventureGame.Engine.DSL.Evaluation;

using AdventureGame.Engine.DSL.AST;

/// <summary>
/// Evaluates a DSL condition AST against game state.
/// </summary>
public class DslEvaluator : INodeVisitor
{
    private bool _lastResult = false;
    private readonly DslEvaluationContext _context;

    public DslEvaluator(DslEvaluationContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public bool Evaluate(ConditionNode ast)
    {
        if (ast == null) throw new ArgumentNullException(nameof(ast));
        ast.Accept(this);
        return _lastResult;
    }

    public string Visit(ConditionNode node)
    {
        _lastResult = false;
        return "unknown";
    }

    public string Visit(AndNode node)
    {
        node.Left.Accept(this);
        bool leftResult = _lastResult;

        node.Right.Accept(this);
        bool rightResult = _lastResult;

        _lastResult = leftResult && rightResult;
        return _lastResult ? "true" : "false";
    }

    public string Visit(OrNode node)
    {
        node.Left.Accept(this);
        bool leftResult = _lastResult;

        node.Right.Accept(this);
        bool rightResult = _lastResult;

        _lastResult = leftResult || rightResult;
        return _lastResult ? "true" : "false";
    }

    public string Visit(NotNode node)
    {
        node.Inner.Accept(this);
        _lastResult = !_lastResult;
        return _lastResult ? "true" : "false";
    }

    public string Visit(RelationNode node)
    {
        _lastResult = EvaluateRelation(node);
        return _lastResult ? "true" : "false";
    }

    public string Visit(CountRelationNode node)
    {
        _lastResult = EvaluateCountRelation(node);
        return _lastResult ? "true" : "false";
    }

    public string Visit(DistanceRelationNode node)
    {
        _lastResult = EvaluateDistanceRelation(node);
        return _lastResult ? "true" : "false";
    }

    private bool EvaluateRelation(RelationNode node)
    {
        // Get the subject value
        var subjectValue = GetSubjectValue(node.Subject);

        // Handle special relations
        if (node.Relation.Equals("is_empty", StringComparison.OrdinalIgnoreCase))
        {
            return subjectValue == null || (subjectValue is string s && string.IsNullOrEmpty(s)) ||
                   (subjectValue is System.Collections.ICollection c && c.Count == 0);
        }

        if (node.Relation.Equals("is_in", StringComparison.OrdinalIgnoreCase))
        {
            // Check if subject is in object's inventory
            var container = GetElementValue(node.Object.Value);
            if (container is Dictionary<string, object> containerDict && containerDict.ContainsKey("inventory"))
            {
                var inventory = containerDict["inventory"];
                if (inventory is System.Collections.Generic.List<object> list)
                {
                    return list.Contains(subjectValue);
                }
            }
            return false;
        }

        // Numeric and equality comparisons
        return CompareValues(subjectValue, node.Object, node.Relation);
    }

    private bool EvaluateCountRelation(CountRelationNode node)
    {
        int visitCount = _context.GetVisitCount(node.Subject.Kind, node.SceneName);
        return Compare(visitCount, node.Value, node.Comparison);
    }

    private bool EvaluateDistanceRelation(DistanceRelationNode node)
    {
        int distance = _context.GetDistance(node.SubjectA, node.SubjectB);
        return Compare(distance, node.Value, node.Comparison);
    }

    private bool CompareValues(object? subjectValue, ObjectRef objectRef, string relation)
    {
        object? objectValue = objectRef.BoolValue != null ? objectRef.BoolValue :
                              objectRef.NumericValue != null ? objectRef.NumericValue :
                              objectRef.Value;

        if (relation.Equals("is", StringComparison.OrdinalIgnoreCase))
        {
            return Equals(subjectValue, objectValue);
        }

        if (relation.Equals("is_not", StringComparison.OrdinalIgnoreCase))
        {
            return !Equals(subjectValue, objectValue);
        }

        // Numeric comparisons
        if (subjectValue is IComparable comparable && objectValue is IComparable comparableObj)
        {
            return relation.ToLower() switch
            {
                "is_less_than" => comparable.CompareTo(comparableObj) < 0,
                "is_greater_than" => comparable.CompareTo(comparableObj) > 0,
                "is_equal_to" => comparable.CompareTo(comparableObj) == 0,
                "is_not_equal_to" => comparable.CompareTo(comparableObj) != 0,
                _ => false
            };
        }

        return false;
    }

    private bool Compare(int left, int right, string op)
    {
        return op.ToLower() switch
        {
            "is_less_than" => left < right,
            "is_greater_than" => left > right,
            "is_equal_to" => left == right,
            "is_not_equal_to" => left != right,
            _ => false
        };
    }

    private object? GetSubjectValue(SubjectRef subject)
    {
        return subject.Kind.ToLower() switch
        {
            "player" => _context.GetPlayer(),
            "target" => _context.GetTarget(),
            "target2" => _context.GetTarget2(),
            "currentscene" => _context.GetCurrentScene(),
            "session" => _context.GetSession(),
            "log" => _context.GetLog(),
            "item" => _context.GetElement("item", subject.Id),
            "npc" => _context.GetElement("npc", subject.Id),
            "scene" => _context.GetElement("scene", subject.Id),
            "exit" => _context.GetElement("exit", subject.Id),
            _ => null
        };
    }

    private object? GetElementValue(string id)
    {
        return _context.GetElement("item", id) ?? 
               _context.GetElement("npc", id) ?? 
               _context.GetElement("scene", id) ?? 
               _context.GetElement("exit", id);
    }
}

/// <summary>
/// Context for DSL evaluation, providing access to game state.
/// </summary>
public abstract class DslEvaluationContext
{
    public abstract object? GetPlayer();
    public abstract object? GetTarget();
    public abstract object? GetTarget2();
    public abstract object? GetCurrentScene();
    public abstract object? GetSession();
    public abstract object? GetLog();
    public abstract object? GetElement(string kind, string? id);
    public abstract int GetVisitCount(string subjectKind, string sceneName);
    public abstract int GetDistance(SubjectRef from, SubjectRef to);
}
