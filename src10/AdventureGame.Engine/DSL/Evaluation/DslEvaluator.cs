namespace AdventureGame.Engine.DSL.Evaluation;

using AdventureGame.Engine.DSL.AST;
using AdventureGame.Engine.Models.Elements;

/// <summary>
/// Evaluates a DSL condition AST against game state.
/// </summary>
public class DslEvaluator(DslEvaluationContext context) : INodeVisitor
{
    private bool _lastResult = false;
    private readonly DslEvaluationContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public bool Evaluate(ConditionNode ast)
    {
        ArgumentNullException.ThrowIfNull(ast);
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
        // Handle "has" relation - inventory containment check
        if (node.Relation.Equals("has", StringComparison.OrdinalIgnoreCase))
        {
            return EvaluateHasRelation(node);
        }

        // Get the subject value
        var subjectValue = GetSubjectValue(node.Subject, node.AttributeName, node.PropertyName);

        // If propertyName is null (implicit property), try to infer it from the comparison value
        if (node.PropertyName == null && subjectValue == null && node.Subject.Kind.ToLower() != "player" &&
            node.Subject.Kind.ToLower() != "target" && node.Subject.Kind.ToLower() != "target2" &&
            node.Subject.Kind.ToLower() != "currentscene" && node.Subject.Kind.ToLower() != "location" &&
            node.Subject.Kind.ToLower() != "session" && node.Subject.Kind.ToLower() != "log")
        {
            // This is a named element (item, npc, scene, exit) without explicit property
            // Try to infer from the comparison value
            if (GetSubjectValue(node.Subject, null, null) is AdventureGame.Engine.Models.GameElement element && node.Object.Value != null)
            {
                // Try flags first, then state
                subjectValue = GetGameElementPropertyWithInference(element, node.Object.Value);
            }
        }

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
            if (container is Dictionary<string, object> containerDict && containerDict.TryGetValue("inventory", out object? inventory))
            {
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

    /// <summary>
    /// Evaluates a "has" relation - checks if subject contains/has the object.
    /// Examples: "player has key", "npc has sword", "target has item"
    /// </summary>
    private bool EvaluateHasRelation(RelationNode node)
    {
        // Get the container (subject)
        var container = GetSubjectValue(node.Subject, null, null);
        if (container == null) return false;

        // Get the item to check for (object)
        var itemToFind = node.Object.Value;
        if (string.IsNullOrEmpty(itemToFind)) return false;

        // Get the actual item object
        var itemElement = GetElementOrSpecialValue(itemToFind);

        // Check if container is a GameElement (item or NPC)
        if (container is AdventureGame.Engine.Models.GameElement containerElement)
        {
            // Look for item in the container's children (items that have this as parent)
            if (_context is GameSessionDslContext gameContext && gameContext.Session != null)
            {
                // Use Session.Elements (runtime state) instead of Session.Pack.Elements (default state)
                var itemsInContainer = gameContext.Session.Elements
                    .OfType<Item>()
                    .Where(item => item.ParentId == containerElement.Id)
                    .ToList();

                if (itemsInContainer != null && itemsInContainer.Any())
                {
                    // Check if the requested item is in the container
                    if (itemElement is AdventureGame.Engine.Models.GameElement itemElementTyped)
                    {
                        return itemsInContainer.Any(i => i.Id.Value == itemElementTyped.Id.Value);
                    }
                    // Also check by name
                    return itemsInContainer.Any(i => i.Name.Equals(itemToFind, StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        return false;
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
        // Resolve the object value
        object? objectValue;
        if (objectRef.BoolValue != null)
        {
            objectValue = objectRef.BoolValue;
        }
        else if (objectRef.NumericValue != null)
        {
            objectValue = objectRef.NumericValue;
        }
        else if (objectRef.Kind == "element")
        {
            // Resolve element references (e.g., "target", "player", etc.)
            objectValue = GetElementOrSpecialValue(objectRef.Value);
            // If not resolvable as an element/special, fall back to raw string for comparisons like state == "open"
            objectValue ??= objectRef.Value;
        }
        else
        {
            objectValue = objectRef.Value;
        }

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

    private object? GetElementOrSpecialValue(string value)
    {
        // Try special subjects first
        var valueLower = value.ToLower();
        return valueLower switch
        {
            "player" => _context.GetPlayer(),
            "target" => _context.GetTarget(),
            "target2" => _context.GetTarget2(),
            "currentscene" => _context.GetCurrentScene(),
            "session" => _context.GetSession(),
            "log" => _context.GetLog(),
            _ => GetElementValue(value) // Otherwise try as element ID
        };
    }

    private object? GetElementValue(string id)
    {
        return _context.GetElement("item", id) ?? 
               _context.GetElement("npc", id) ?? 
               _context.GetElement("scene", id) ?? 
               _context.GetElement("exit", id);
    }

    private static bool Compare(int left, int right, string op)
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

    private object? GetSubjectValue(SubjectRef subject, string? attributeName = null, string? propertyName = null)
    {
        var baseValue = subject.Kind.ToLower() switch
        {
            "player" => _context.GetPlayer(),
            "target" => _context.GetTarget(),
            "target2" => _context.GetTarget2(),
            "currentscene" => _context.GetCurrentScene(),
            "location" => _context.GetCurrentScene(),  // "location" is an alias for currentScene
            "session" => _context.GetSession(),
            "log" => _context.GetLog(),
            "item" => _context.GetElement("item", subject.Id),
            "npc" => _context.GetElement("npc", subject.Id),
            "scene" => _context.GetElement("scene", subject.Id),
            "exit" => _context.GetElement("exit", subject.Id),
            _ => null
        };

        // If no property/attribute access, return the base value
        if (string.IsNullOrEmpty(attributeName) && string.IsNullOrEmpty(propertyName))
        {
            return baseValue;
        }

        // Handle GameElement property/attribute access
        if (baseValue is AdventureGame.Engine.Models.GameElement element)
        {
            // Access attribute
            if (!string.IsNullOrEmpty(attributeName))
            {
                if (element.Attributes.TryGetValue(attributeName, out var attrValue))
                {
                    return attrValue;
                }
                return null;
            }

            // Access property - when propertyName is provided explicitly
            if (!string.IsNullOrEmpty(propertyName))
            {
                return GetGameElementProperty(element, propertyName);
            }
        }

        // Try to access property/attribute from dictionary (backward compatibility)
        if (baseValue is Dictionary<string, object> dict)
        {
            if (!string.IsNullOrEmpty(attributeName) && dict.TryGetValue("attributes", out var attrs))
            {
                if (attrs is Dictionary<string, object> attrDict && attrDict.TryGetValue(attributeName, out var attrValue))
                {
                    return attrValue;
                }
            }

            if (!string.IsNullOrEmpty(propertyName) && dict.TryGetValue(propertyName, out var propValue))
            {
                return propValue;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a property/flag/state value from a GameElement, with intelligent fallback.
    /// When property is null (e.g., "desk is open"), checks:
    /// 1. Flags first (e.g., "open" flag)
    /// 2. Then States (e.g., current state == "open")
    /// </summary>
    private object? GetGameElementProperty(AdventureGame.Engine.Models.GameElement element, string propertyName)
    {
        // Special handling for "state" property - always check state
        if (propertyName.Equals("state", StringComparison.OrdinalIgnoreCase))
        {
            // Check CurrentState first, then DefaultState
                if (element.Properties.TryGetValue("CurrentState", out var currentState) && !string.IsNullOrWhiteSpace(currentState))
            {
                return currentState;
            }
            return element.DefaultState;
        }

        // For other properties, check in this order:
        // 1. Properties dictionary (custom properties)
        if (element.Properties.TryGetValue(propertyName, out var propValue))
        {
            return propValue;
        }

        // 2. Flags dictionary (boolean flags like "isMovable", "isVisible")
        if (element.Flags.TryGetValue(propertyName, out var flagValue))
        {
            return flagValue;
        }

        // 3. Try as a state name (legacy fallback)
        if (element.States.ContainsKey(propertyName))
        {
            return propertyName; // Return the state name itself
        }

        return null;
    }

    /// <summary>
    /// Gets a property from a GameElement when the property name is inferred from the comparison value.
    /// Example: "desk is open" - infers that we're checking either a flag named "open" or state "open"
    /// Checks in order:
    /// 1. Flag matching the value (e.g., "open" flag)
    /// 2. Current state matches the value (e.g., state == "open")
    /// </summary>
    private object? GetGameElementPropertyWithInference(AdventureGame.Engine.Models.GameElement element, string inferredPropertyName)
    {
        // Try to find a matching flag
        if (element.Flags.TryGetValue(inferredPropertyName, out var flagValue))
        {
            return flagValue;
        }

        // Check if it's a valid state and return the current state
        if (element.States.ContainsKey(inferredPropertyName) ||
            element.DefaultState.Equals(inferredPropertyName, StringComparison.OrdinalIgnoreCase))
        {
            // Return current state if set, otherwise default
            if (element.Properties.TryGetValue("CurrentState", out var currentState) && !string.IsNullOrWhiteSpace(currentState))
            {
                return currentState;
            }
            return element.DefaultState;
        }

        // Try property dictionary as fallback
        if (element.Properties.TryGetValue(inferredPropertyName, out var propValue))
        {
            return propValue;
        }

        // If all else fails, return null - the comparison will fail appropriately
        return null;
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

/// <summary>
/// Extended context that provides access to the GameSession for more detailed operations.
/// </summary>
public class GameSessionDslContext : DslEvaluationContext
{
    public AdventureGame.Engine.Runtime.GameSession? Session { get; set; }

    public override object? GetPlayer() => Session?.Player;
    public override object? GetTarget() => null;
    public override object? GetTarget2() => null;
    public override object? GetCurrentScene() => Session?.CurrentScene;
    public override object? GetSession() => Session;
    public override object? GetLog() => Session?.History;
    
    public override object? GetElement(string kind, string? id)
    {
        if (string.IsNullOrWhiteSpace(id) || Session?.Elements == null)
            return null;

        // Use Session.Elements (runtime state) instead of Session.Pack.Elements (default state)
        return Session.Elements.FirstOrDefault(e => 
            e.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase) && 
            (e.Id.ToString() == id || e.Name.Equals(id, StringComparison.OrdinalIgnoreCase)));
    }
    
    public override int GetVisitCount(string subjectKind, string sceneName) => 0;
    public override int GetDistance(AdventureGame.Engine.DSL.AST.SubjectRef from, AdventureGame.Engine.DSL.AST.SubjectRef to) => 0;
}
