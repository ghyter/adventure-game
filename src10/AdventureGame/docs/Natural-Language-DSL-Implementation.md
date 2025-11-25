# Natural Language Effects DSL - Implementation Summary

## Overview

This implementation provides a comprehensive natural language DSL for authoring game effects and conditions without requiring dot notation or technical syntax.

## ? Components Implemented

### 1. Semantic Property Resolver
**File**: `AdventureGame.Engine/Semantics/SemanticPropertyResolver.cs`

**Purpose**: Maps natural language semantic names (e.g., "open", "closed", "lit") to actual element fields.

**Resolution Precedence** (highest ? lowest):
1. **States** - Named states like "open", "closed"
2. **Flags** - Boolean flags like "visible", "locked"
3. **Properties** - String properties
4. **Attributes** - Numeric attributes like "health", "stamina"

**Key Methods**:
- `Resolve(element, semanticName)` - Find the binding for a semantic name
- `GetValue(element, binding)` - Get current value
- `SetValue(element, binding, value)` - Set new value
- `Increment(element, binding, amount)` - Increase numeric value
- `Decrement(element, binding, amount)` - Decrease numeric value

### 2. Effect AST Nodes
**File**: `AdventureGame.Engine/Effects/EffectNodes.cs`

**Supported Effect Types**:

| Effect Type | Example | Description |
|-------------|---------|-------------|
| `SetSemanticEffect` | `set target to open` | Semantic resolution |
| `SetStateEffect` | `set target state to open` | Explicit state |
| `SetFlagEffect` | `set target visible to true` | Boolean flag |
| `SetPropertyEffect` | `set target description to 'old door'` | String property |
| `SetAttributeEffect` | `set player health to 10` | Numeric attribute |
| `IncrementEffect` | `increment player stamina by 1` | Increase number |
| `DecrementEffect` | `decrement target battery by 5` | Decrease number |
| `DescribeEffect` | `describe currentScene` | Show description |
| `PrintEffect` | `print 'The door opens'` | Print message |
| `TeleportEffect` | `teleport target` | Travel through exit |
| `AddToInventoryEffect` | `add target to inventory` | Pick up item |
| `RemoveFromInventoryEffect` | `remove target from inventory` | Drop item |

### 3. Condition AST Nodes
**File**: `AdventureGame.Engine/Conditions/ConditionNodes.cs`

**Supported Condition Types**:

| Condition Type | Example | Description |
|-------------|---------|-------------|
| `KindCheckCondition` | `target is exit` | Check element type |
| `StateCheckCondition` | `target is open` | Check current state |
| `FlagCheckCondition` | `target visible is true` | Check boolean flag |
| `PropertyCheckCondition` | `target name is 'door'` | Check string property |
| `AttributeCheckCondition` | `player health is 10` | Check exact number |
| `ComparisonCondition` | `player health > 5` | Numeric comparison |
| `HasItemCondition` | `player has key` | Check inventory |
| `LogicalAnd` | Combines conditions with AND |
| `LogicalOr` | Combines conditions with OR |
| `LogicalNot` | Negates a condition |

**Comparison Operators**:
- `>` - Greater than
- `<` - Less than
- `>=` - Greater than or equal
- `<=` - Less than or equal
- `==` or `=` - Equal
- `!=` - Not equal

### 4. Natural Effect Parser
**File**: `AdventureGame.Engine/Parser/NaturalEffectParser.cs`

**Supported Patterns**:

```
set <target> to <semantic>
set <target> <property> to <value>
increment <target> <key> by <amount>
decrement <target> <key> by <amount>
describe <target>
show <target>
print "<message>"
teleport <target>
add <target> to inventory
remove <target> from inventory
```

**Key Methods**:
- `Parse(statement)` - Parse single statement
- `ParseMultiple(statements)` - Parse multiple lines
- `Validate(statement)` - Check syntax

### 5. Natural Condition Parser
**File**: `AdventureGame.Engine/Parser/NaturalConditionParser.cs`

**Supported Patterns**:

```
<subject> is <type>
<subject> is <state>
<subject> has <item>
<subject> <property> <comparison> <value>
<subject> <property> is <value>
```

**Logical Operators**:
- ` and ` - Combine conditions with AND
- ` or ` - Combine conditions with OR

**Key Methods**:
- `Parse(statement)` - Parse single condition
- `ParseMultiple(statements)` - Parse with AND/OR
- `Validate(statement)` - Check syntax

### 6. Action Executor (Extended)
**File**: `AdventureGame.Engine/Actions/DescribeAction.cs`

**Purpose**: Execute natural language effects on game sessions.

**New Methods**:
- `Execute(effect, session, target, target2)` - Main execution dispatcher
- `ResolveSubject(subject, session, target, target2)` - Resolve target identifiers
- `ExecuteTeleport(...)` - Implement exit teleportation
- `ExecuteAddToInventory(...)` - Implement item pickup
- `ExecuteRemoveFromInventory(...)` - Implement item drop
- Plus execution methods for all effect types

**Supported Target Identifiers**:
- `player` or `currentPlayer` - The active player
- `currentScene` - Current scene
- `target` or `target1` - First verb target
- `target2` - Second verb target
- Any element name or alias

## Usage Examples

### Effect Examples

#### Simple State Change
```
set target to open
```
? Changes target's state to "open" using semantic resolution

#### Explicit Property Set
```
set player health to 10
```
? Sets player.Attributes["health"] = 10

#### Increment Attribute
```
increment player stamina by 1
```
? Increases player stamina by 1

#### Describe Scene
```
describe currentScene
```
? Shows scene name, description, and current state description

#### Teleport Through Exit
```
teleport target
```
? Moves player through exit to paired exit's scene

#### Add to Inventory
```
add target to inventory
```
? Sets target.ParentId to player.Id

### Condition Examples

#### Type Check
```
target is exit
```
? Checks if target.Kind == "exit"

#### State Check
```
target is open
```
? Checks if target's current state is "open"

#### Inventory Check
```
player has key
```
? Checks if any item with name "key" has ParentId == player.Id

#### Numeric Comparison
```
player health > 5
```
? Checks if player.Attributes["health"] > 5

#### Complex Condition
```
player has key and target is locked
```
? Both conditions must be true

## Natural Language Benefits

### Before (Technical):
```csharp
effect.Action = "ChangeState:door:open"
effect.Action = "MoveTo:target:player"
```

### After (Natural Language):
```
set target to open
add target to inventory
```

### Advantages:
1. **No Dot Notation** - Authors don't need to know property paths
2. **Semantic Resolution** - "open" maps to the right field automatically
3. **Clear Intent** - Reads like plain English
4. **Flexible** - Works with states, flags, properties, attributes
5. **Forgiving** - Handles missing values gracefully

## Semantic Resolution Examples

Given an element with:
- States: { "open": {...}, "closed": {...} }
- Flags: { "visible": true, "locked": false }
- Properties: { "description": "An old door" }
- Attributes: { "durability": 10 }

**Statement**: `set target to open`
- Resolver checks States first
- Finds "open" state
- Sets CurrentState = "open"

**Statement**: `set target visible to false`
- Resolver checks Flags
- Finds "visible" flag
- Sets visible = false

**Statement**: `increment target durability by 1`
- Resolver checks Attributes
- Finds "durability"
- Increments from 10 to 11

## Teleport Implementation

**Natural Statement**:
```
teleport target
```

**What Happens**:
1. Resolve `target` to an Exit element
2. Find paired exit via `TargetExitId`
3. Find scene containing paired exit
4. Set `player.ParentId` to that scene's ID
5. Update `session.CurrentScene`
6. Return success message

**Example**:
```
"You travel through North Door and arrive at Hallway."
```

## Inventory Implementation

**Add to Inventory**:
```
add target to inventory
```
? Sets `target.ParentId = player.Id`

**Remove from Inventory**:
```
remove target from inventory
```
? Sets `target.ParentId = currentScene.Id`

**Check Inventory (Condition)**:
```
player has key
```
? Searches elements where:
- `element.Name == "key"` or `element.Aliases.Contains("key")`
- `element.ParentId == player.Id`

## Integration Points

### With Verb System
Verbs can now use natural language effects:
```csharp
new Verb
{
    Name = "open",
    TargetCount = 1,
    ConditionTexts = ["target is door", "target is closed"],
    Effects = new()
    {
        new VerbEffect
        {
            SuccessText = "The door swings open.",
            Action = "set target to open"  // Natural language!
        }
    }
}
```

### With Condition Tester
Test natural language conditions:
```
when player health > 5 and player has sword
```

### With Effect Tester
Test natural language effects:
```
set target to open
add target to inventory
teleport target
```

## Error Handling

All executors provide clear error messages:

```
"Error: Could not find element 'foo'"
"Error: door is not an exit"
"Error: Exit north has no paired exit"
"Error: Could not increment stamina on player"
```

## Future Enhancements

### Potential Additions
1. **Variable Substitution** - `{player.name}` in messages
2. **Conditional Effects** - `if player health < 5 then set player health to 5`
3. **Loops** - `for each item in scene do describe item`
4. **Custom Functions** - `roll 1d20` for dice rolls
5. **Effect Chaining** - Multiple effects in sequence

### Parser Improvements
1. **Fuzzy Matching** - Handle typos
2. **Synonyms** - "put" = "add", "drop" = "remove"
3. **Shortcuts** - "grab key" = "add key to inventory"
4. **Contextual Defaults** - "open" assumes "target" if omitted

## Architecture Benefits

### Type Safety
- All AST nodes are strongly typed
- Semantic resolver enforces valid operations
- No runtime string evaluation

### Performance
- Parse once, execute many times
- Compiled AST structure
- Fast semantic lookup (dictionary-based)

### Extensibility
- Easy to add new effect types
- Easy to add new condition types
- Parser patterns are regex-based and modular

### Maintainability
- Clear separation: Parser ? AST ? Executor
- Single source of truth for semantic resolution
- Comprehensive error messages

## Build Status

? **Build Successful**
- No compilation errors
- All new components integrated
- Type-safe throughout

## Files Created

1. `AdventureGame.Engine/Semantics/SemanticPropertyResolver.cs`
2. `AdventureGame.Engine/Effects/EffectNodes.cs`
3. `AdventureGame.Engine/Conditions/ConditionNodes.cs`
4. `AdventureGame.Engine/Parser/NaturalEffectParser.cs`
5. `AdventureGame.Engine/Parser/NaturalConditionParser.cs`

## Files Modified

1. `AdventureGame.Engine/Actions/DescribeAction.cs` - Extended ActionExecutor

## Summary

The Natural Language Effects DSL system is **fully implemented** and provides:

? **Semantic Property Resolution** - Maps natural names to actual fields
? **12 Effect Types** - Comprehensive effect authoring
? **10 Condition Types** - Rich condition checking
? **Natural Language Parsers** - Easy-to-use syntax
? **Teleport & Inventory** - Special actions implemented
? **Error Handling** - Clear, actionable messages
? **Type Safety** - Strongly typed AST
? **Extensibility** - Easy to add new features

**Ready for Integration** with UI components (EffectTester, VerbEditor, etc.) ?
