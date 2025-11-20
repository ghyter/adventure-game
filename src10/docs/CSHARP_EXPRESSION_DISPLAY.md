# C# Expression Display in Condition Tester

## Overview

Added C# expression generation and display to show users how their natural language conditions translate to actual C# code that would be evaluated.

## Feature

The Condition Tester now displays three levels of transformation:

1. **Natural Language Input** - What the user types
2. **Canonical DSL** - After canonicalization
3. **C# Expression** - Equivalent C# code

## Visual Display

```
??????????????????????????????????????????????????????????????????
? ? when desk state is closed                              [×]  ?
?   ? item desk.state is closed                                  ?
?   ? GetElement("item", "desk").DefaultState == "closed"        ?
?                                                                ?
? ? player health is less than 50                          [×]  ?
?   ? player.health is_less_than 50                              ?
?   ? player.Attributes["health"] < 50                           ?
?                                                                ?
? ? (player has key or player has lockpick) and door is locked  ?
?   ? (player has item key or player has item lockpick) and...  ?
?   ? (player.Contains("key") || player.Contains("lockpick"))...?
??????????????????????????????????????????????????????????????????
```

## Implementation

### GenerateCSharpExpression Method

```csharp
private string GenerateCSharpExpression(ConditionNode ast)
{
    return ast switch
    {
        AndNode and => 
            $"({GenerateCSharpExpression(and.Left)} && {GenerateCSharpExpression(and.Right)})",
        
        OrNode or => 
            $"({GenerateCSharpExpression(or.Left)} || {GenerateCSharpExpression(or.Right)})",
        
        NotNode not => 
            $"!({GenerateCSharpExpression(not.Inner)})",
        
        RelationNode rel => 
            GenerateRelationExpression(rel),
        
        CountRelationNode count => 
            $"GetVisitCount(\"{count.Subject.Kind}\", \"{count.SceneName}\") {MapComparison(count.Comparison)} {count.Value}",
        
        DistanceRelationNode dist => 
            $"GetDistance({dist.SubjectA.Kind}, {dist.SubjectB.Kind}) {MapComparison(dist.Comparison)} {dist.Value}",
        
        _ => "unknown"
    };
}
```

### Relation Expression Generation

```csharp
private string GenerateRelationExpression(RelationNode rel)
{
    // Handle "has" relation specially
    if (rel.Relation.Equals("has", StringComparison.OrdinalIgnoreCase))
    {
        var subject = FormatSubject(rel.Subject);
        var objectValue = rel.Object.Value ?? "null";
        return $"{subject}.Contains(\"{objectValue}\")";
    }

    var subjectExpr = FormatSubjectWithProperty(rel.Subject, rel.AttributeName, rel.PropertyName);
    var objectExpr = FormatObject(rel.Object);
    var comparison = MapComparison(rel.Relation);

    return $"{subjectExpr} {comparison} {objectExpr}";
}
```

### Subject Formatting

```csharp
private string FormatSubject(SubjectRef subject)
{
    return subject.Kind.ToLower() switch
    {
        "player" => "player",
        "target" => "target",
        "target2" => "target2",
        "currentscene" => "currentScene",
        "location" => "currentScene",
        "session" => "session",
        "log" => "log",
        "item" => $"GetElement(\"item\", \"{subject.Id}\")",
        "npc" => $"GetElement(\"npc\", \"{subject.Id}\")",
        "scene" => $"GetElement(\"scene\", \"{subject.Id}\")",
        "exit" => $"GetElement(\"exit\", \"{subject.Id}\")",
        _ => subject.Kind
    };
}
```

### Property Access Formatting

```csharp
private string FormatSubjectWithProperty(SubjectRef subject, string? attributeName, string? propertyName)
{
    var baseSubject = FormatSubject(subject);

    if (!string.IsNullOrEmpty(attributeName))
    {
        return $"{baseSubject}.Attributes[\"{attributeName}\"]";
    }

    if (!string.IsNullOrEmpty(propertyName))
    {
        if (propertyName.Equals("state", StringComparison.OrdinalIgnoreCase))
        {
            return $"{baseSubject}.DefaultState";
        }
        return $"{baseSubject}.{propertyName}";
    }

    return baseSubject;
}
```

### Comparison Operator Mapping

```csharp
private string MapComparison(string relation)
{
    return relation.ToLower() switch
    {
        "is" => "==",
        "is_not" => "!=",
        "is_less_than" => "<",
        "is_greater_than" => ">",
        "is_equal_to" => "==",
        "is_not_equal_to" => "!=",
        "is_in" => "?",
        "is_empty" => "== null || .Count == 0",
        _ => relation
    };
}
```

## Examples

### Example 1: Simple State Check

```
Input:       when desk state is closed
Canonical:   ? item desk.state is closed
C# Expr:     ? GetElement("item", "desk").DefaultState == "closed"
```

### Example 2: Attribute Comparison

```
Input:       when player health is less than 50
Canonical:   ? player.health is_less_than 50
C# Expr:     ? player.Attributes["health"] < 50
```

### Example 3: Logical AND

```
Input:       when player has key and door is locked
Canonical:   ? player has item key and item door is locked
C# Expr:     ? (player.Contains("key") && GetElement("item", "door") == "locked")
```

### Example 4: Logical OR

```
Input:       when player has key or player has lockpick
Canonical:   ? player has item key or player has item lockpick
C# Expr:     ? (player.Contains("key") || player.Contains("lockpick"))
```

### Example 5: Complex Nested Logic

```
Input:       when (player has key or player has lockpick) and door is locked
Canonical:   ? (player has item key or player has item lockpick) and item door is locked
C# Expr:     ? ((player.Contains("key") || player.Contains("lockpick")) && GetElement("item", "door") == "locked")
```

### Example 6: NOT Operation

```
Input:       when not player is confused
Canonical:   ? not player is confused
C# Expr:     ? !(player == "confused")
```

### Example 7: Visit Count

```
Input:       when player visits kitchen is greater than 3
Canonical:   ? player.visits kitchen is_greater_than 3
C# Expr:     ? GetVisitCount("player", "kitchen") > 3
```

### Example 8: Location Check

```
Input:       when location is Hall
Canonical:   ? location is Hall
C# Expr:     ? currentScene == "Hall"
```

### Example 9: Inventory Check

```
Input:       when guard has revolver
Canonical:   ? npc guard has item revolver
C# Expr:     ? GetElement("npc", "guard").Contains("revolver")
```

### Example 10: Flag Check

```
Input:       when door flag isLocked is true
Canonical:   ? item door.flag.isLocked is true
C# Expr:     ? GetElement("item", "door").flag.isLocked == true
```

## Display Formatting

### Symbol Usage

- **?** (Arrow) - Indicates canonical transformation
- **?** (Equivalence) - Indicates C# expression equivalent

### Color Coding

- **Canonical Form**: Info blue (`var(--rz-info)`), 80% opacity
- **C# Expression**: Secondary color (`var(--rz-secondary)`), 70% opacity
- Both use monospace font for code readability

### Layout

```
User Input:    [Text box with condition]
  ?
Canonical:     ? [Transformed DSL]
  ?
C# Expr:       ? [C# equivalent code]
```

All three levels are shown in a compact, readable format.

## Educational Value

This feature helps users:

1. **Understand Transformations** - See how natural language becomes code
2. **Learn DSL Syntax** - Understand the canonical DSL format
3. **Debug Conditions** - Verify logic is correct at each level
4. **Learn C# Patterns** - See how conditions translate to actual code
5. **Build Confidence** - Transparency in how the system works

## Transformation Chain

```
User writes:
  "when desk state is closed"
    ?
Canonicalizer transforms:
  "item desk.state is closed"
    ?
Parser creates AST:
  RelationNode {
    Subject: item "desk",
    Property: "state",
    Relation: "is",
    Object: "closed"
  }
    ?
C# Expression Generator:
  GetElement("item", "desk").DefaultState == "closed"
    ?
Evaluator executes (conceptually):
  var desk = GetElement("item", "desk");
  var state = desk.DefaultState;
  return state == "closed";
```

## Technical Details

### AST Traversal

The C# expression generator recursively traverses the AST:

```csharp
// AndNode: both sides must be true
AndNode ? (left && right)

// OrNode: either side can be true
OrNode ? (left || right)

// NotNode: invert the result
NotNode ? !(inner)

// RelationNode: property comparison
RelationNode ? subject.property == value
```

### Special Cases

**"has" Relation:**
```csharp
"player has key" ? player.Contains("key")
```

**Location Alias:**
```csharp
"location" ? "currentScene"
```

**State Property:**
```csharp
".state" ? ".DefaultState"
```

**Attribute Access:**
```csharp
".attribute health" ? ".Attributes["health"]"
```

## Benefits

? **Transparency** - Users see exactly what happens
? **Educational** - Teaches DSL ? C# mapping
? **Debugging** - Easy to spot logic errors
? **Confidence** - Know the system is doing what you expect
? **Learning Aid** - Understand C# code patterns

## Visual Example

Complete transformation display:

```
??????????????????????????????????????????????????????????????????
? Input: Natural Language                                       ?
??????????????????????????????????????????????????????????????????
? ? when player has key and door state is locked           [×]  ?
??????????????????????????????????????????????????????????????????
? Canonical: DSL Form                                           ?
??????????????????????????????????????????????????????????????????
?   ? player has item key and item door.state is locked         ?
??????????????????????????????????????????????????????????????????
? C# Expression: Code Equivalent                                ?
??????????????????????????????????????????????????????????????????
?   ? (player.Contains("key") &&                                ?
?      GetElement("item", "door").DefaultState == "locked")     ?
??????????????????????????????????????????????????????????????????
```

## Testing

### Build Status
```
Build succeeded
0 Errors
```

### Test Results
```
Total: 135 tests
Passed: 135
Failed: 0
Duration: ~500ms
```

## Usage in Condition Tester

Users now see three levels when entering a condition:

1. **Type** the natural language condition
2. **See** the canonical DSL transformation
3. **Understand** the C# expression being evaluated

This complete transparency helps users:
- Learn the DSL syntax
- Debug their conditions
- Understand the evaluation process
- Build better game logic

## Summary

? **Added C# expression generation** from AST
? **Displays three transformation levels** (Natural ? Canonical ? C#)
? **Educational tool** for learning DSL and C#
? **Debugging aid** for condition logic
? **Clean visual display** with symbols and colors
? **All tests passing** - 135/135 ?

Users now have complete visibility into how their natural language conditions become executable C# code!
