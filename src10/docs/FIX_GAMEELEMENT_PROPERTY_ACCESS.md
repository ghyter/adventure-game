# Fix: GameElement Property Access in DSL Evaluator

## Problem

The DSL evaluator wasn't properly accessing GameElement properties. When you wrote:
- `"desk.state is closed"` 
- `"when desk state is closed"`

The evaluator was trying to access properties from a `Dictionary<string, object>`, but GameElement is a class with typed properties (`Properties`, `Attributes`, `Flags`, `States`).

## Root Cause

```csharp
// OLD CODE (broken)
private object? GetSubjectValue(SubjectRef subject, string? propertyName = null)
{
    var baseValue = /* resolve subject */;
    
    // This only worked for dictionaries!
    if (baseValue is Dictionary<string, object> dict)
    {
        if (dict.TryGetValue(propertyName, out var propValue))
            return propValue;
    }
    
    return null; // ? GameElement properties never accessed!
}
```

## Solution

### 1. Enhanced DslEvaluator

Added proper GameElement property access:

```csharp
private object? GetSubjectValue(SubjectRef subject, string? attributeName = null, string? propertyName = null)
{
    var baseValue = /* resolve subject */;

    // Handle GameElement property/attribute access
    if (baseValue is AdventureGame.Engine.Models.GameElement element)
    {
        // Access attribute from Attributes dictionary
        if (!string.IsNullOrEmpty(attributeName))
        {
            if (element.Attributes.TryGetValue(attributeName, out var attrValue))
                return attrValue;
            return null;
        }

        // Access property
        if (!string.IsNullOrEmpty(propertyName))
        {
            // Special handling for "state" property
            if (propertyName.Equals("state", StringComparison.OrdinalIgnoreCase))
            {
                // Check CurrentState first, then DefaultState
                if (element.Properties.TryGetValue("CurrentState", out var currentState) 
                    && !string.IsNullOrWhiteSpace(currentState))
                {
                    return currentState;
                }
                return element.DefaultState;
            }

            // Check Properties dictionary
            if (element.Properties.TryGetValue(propertyName, out var propValue))
                return propValue;

            // Check Flags dictionary
            if (element.Flags.TryGetValue(propertyName, out var flagValue))
                return flagValue;

            return null;
        }
    }

    // Backward compatibility with dictionary-based contexts
    if (baseValue is Dictionary<string, object> dict) { /* ... */ }

    return null;
}
```

### 2. Enhanced DslVocabulary

Added `ElementKinds` dictionary for accurate type resolution:

```csharp
public sealed class DslVocabulary
{
    // NEW: Maps element names to their actual kinds
    public Dictionary<string, string> ElementKinds { get; } = new(StringComparer.OrdinalIgnoreCase);

    public static DslVocabulary FromGamePack(GamePack pack)
    {
        foreach (var element in pack.Elements)
        {
            var kind = element.Kind.ToLowerInvariant(); // "item", "npc", "scene", etc.
            
            // Register kind for name
            vocab.ElementKinds[element.Name] = kind;
            
            // Register kind for aliases
            foreach (var alias in element.Aliases)
            {
                vocab.ElementKinds[alias] = kind;
            }
        }
    }
}
```

### 3. Enhanced DslCanonicalizer

Use vocabulary for accurate element type determination:

```csharp
private string DetermineElementType(string elementName, DslVocabulary vocab)
{
    // First try: lookup in vocabulary (most accurate!)
    if (vocab?.ElementKinds != null && vocab.ElementKinds.TryGetValue(elementName, out var kind))
    {
        return kind;
    }

    // Fallback: heuristics for common patterns
    if (Regex.IsMatch(elementName, @"\b(guard|knight|wizard|...)\b"))
        return "npc";
    
    if (Regex.IsMatch(elementName, @"\b(room|hall|kitchen|...)\b"))
        return "scene";
    
    // Default to item
    return "item";
}
```

## What Now Works

### Natural Language with Property Access

```
Input: "when desk state is closed"

Processing:
1. Canonicalizer strips "when" ? "desk state is closed"
2. Looks up "desk" in vocabulary ? finds it as "item"
3. Infers subject ? "item desk state is closed"
4. Handles possessive ? "item desk.state is closed"
5. Parser creates AST:
   - Subject: item "desk"
   - Property: "state"
   - Relation: "is"
   - Object: "closed"
6. Evaluator:
   - Resolves "desk" to GameElement instance
   - Accesses element.Properties["CurrentState"] or element.DefaultState
   - Compares to "closed"
   - Returns true/false ?
```

### Property Access Types

**State Property:**
```csharp
"desk.state is closed"
? element.Properties["CurrentState"] ?? element.DefaultState
? "closed"
```

**Attribute Access:**
```csharp
"player.attribute health is_less_than 50"
? element.Attributes["health"]
? 45
```

**Properties Dictionary:**
```csharp
"item.weight is_greater_than 10"
? element.Properties["weight"]
? "15"
```

**Flags Dictionary:**
```csharp
"door.isLocked is true"
? element.Flags["isLocked"]
? true
```

## Example Scenarios

### Scenario 1: Item State Check
```
GameElement:
  Type: Item
  Name: "desk"
  DefaultState: "closed"
  Properties["CurrentState"]: null

Condition: "when desk state is closed"

Evaluation:
1. Canonicalize ? "item desk.state is closed"
2. Parse ? RelationNode { subject: "item desk", property: "state", relation: "is", object: "closed" }
3. Evaluate:
   - Resolve desk ? GameElement instance
   - Get state ? DefaultState = "closed"
   - Compare "closed" == "closed" ? TRUE ?
```

### Scenario 2: NPC Health Check
```
GameElement:
  Type: Npc
  Name: "guard"
  Attributes["health"]: 45

Condition: "when guard health is_less_than 50"

Evaluation:
1. Canonicalize ? "npc guard.attribute health is_less_than 50"
2. Parse ? RelationNode { subject: "npc guard", attribute: "health", relation: "is_less_than", object: 50 }
3. Evaluate:
   - Resolve guard ? GameElement instance
   - Get health attribute ? 45
   - Compare 45 < 50 ? TRUE ?
```

### Scenario 3: Door Lock Status
```
GameElement:
  Type: Item
  Name: "door"
  Flags["isLocked"]: true

Condition: "door isLocked is true"

Evaluation:
1. Canonicalize ? "item door.isLocked is true"
2. Parse ? RelationNode { subject: "item door", property: "isLocked", relation: "is", object: true }
3. Evaluate:
   - Resolve door ? GameElement instance
   - Get isLocked flag ? true
   - Compare true == true ? TRUE ?
```

## Testing

### Test Update

Fixed the `FullPipeline_ParseValidateEvaluate` test to use actual GameElement:

```csharp
[TestMethod]
public void FullPipeline_ParseValidateEvaluate()
{
    var dsl = "player is target and target.state is open";
    var result = _service.ParseAndValidate(dsl);
    Assert.IsTrue(result.Success);
    
    // Create a mock element with state="open"
    var targetElement = new Item { Name = "target" };
    targetElement.Properties["CurrentState"] = "open";
    
    var context = new TestEvaluationContext 
    { 
        Player = targetElement,  // player is target (same object)
        Target = targetElement   // target has state=open
    };
    
    bool evalResult = DslService.Evaluate(result.Ast!, context);
    Assert.IsTrue(evalResult); // ? PASSES!
}
```

### All Tests Pass ?
- **Total**: 135 tests
- **Passed**: 135
- **Failed**: 0
- **Duration**: ~1 second

## Files Modified

1. **AdventureGame.Engine\DSL\Evaluation\DslEvaluator.cs**
   - Enhanced `GetSubjectValue()` to handle GameElement instances
   - Added special handling for `state` property
   - Added Attributes, Properties, and Flags dictionary access
   - Maintained backward compatibility with dictionary contexts

2. **AdventureGame.Engine\DSL\DslVocabulary.cs**
   - Added `ElementKinds` dictionary
   - Populate element kinds from GamePack during vocabulary build
   - Store kinds for both names and aliases

3. **AdventureGame.Engine\DSL\DslCanonicalizer.cs**
   - Updated `DetermineElementType()` to use vocabulary lookup first
   - Fall back to heuristics only when vocabulary doesn't have the element
   - More accurate element type inference

4. **AdventureGame.Engine.Tests\DSL\DslValidationAndEvaluationTests.cs**
   - Fixed `FullPipeline_ParseValidateEvaluate` test
   - Use actual GameElement with state property

## Key Improvements

### ? Accurate Type Resolution
- Uses actual element Kind from GamePack
- No more guessing based on name patterns
- Works with custom element names

### ? Proper Property Access
- Accesses GameElement.Properties dictionary
- Accesses GameElement.Attributes dictionary
- Accesses GameElement.Flags dictionary
- Special handling for `state` property

### ? Natural Language Support
- "when desk state is closed" works correctly
- "desk state is closed" infers "item desk"
- Vocabulary-driven, not heuristic-driven

### ? Backward Compatible
- Still works with dictionary-based test contexts
- Existing tests still pass
- No breaking changes

## Usage Examples

### In Condition Tester

**User Input:**
```
"when desk state is closed"
```

**Processing:**
1. DslService uses DslCanonicalizer
2. Strips "when" prefix
3. Looks up "desk" in vocabulary ? "item"
4. Canonicalizes to "item desk.state is closed"
5. Parser creates AST
6. Evaluator resolves desk GameElement
7. Accesses CurrentState or DefaultState property
8. Compares to "closed"
9. Returns true/false ?

**Alternative Syntax (all work):**
```
"desk state is closed"           ? Natural language
"item desk.state is closed"      ? Explicit type
"desk.state is closed"           ? Implicit item type
"when desk state is closed"      ? With prefix
```

## Summary

? **Fixed**: GameElement property access in evaluator
? **Enhanced**: Vocabulary with element kind tracking
? **Improved**: Canonicalizer uses vocabulary for accurate type determination
? **Tested**: All 135 tests passing
? **Natural Language**: "when desk state is closed" works correctly

**The condition evaluator now properly supports:**
- GameElement.Properties access (state, custom properties)
- GameElement.Attributes access (health, strength, etc.)
- GameElement.Flags access (isLocked, isVisible, etc.)
- Natural language input with accurate element type resolution
- Vocabulary-driven canonicalization
