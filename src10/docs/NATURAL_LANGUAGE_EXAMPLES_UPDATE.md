# Natural Language Condition Examples Update

## Overview

Updated the Condition Examples dialog to display natural language syntax instead of canonical DSL forms. This makes the examples more intuitive and user-friendly.

## Examples by Category

### Natural Language Tab
Clear examples using condition prefixes:
```
when desk state is closed
if player health is less than 50
while guard state is hostile
```

### Implicit Subject Tab
Shows how element names work without type prefixes:
```
desk state is closed
? Automatically becomes ?
item desk state is closed
```

### Operators Tab

**Comparison Operators:**
```
player health is 100
player health is less than 50
player health is greater than 75
```

**Logical Operators:**
```
player has key and door state is locked
player health is less than 20 or player is confused
not chest state is locked
```

### Properties Tab

**State Property:**
```
desk state is closed
door state is not open
```

**Attributes:**
```
player attribute health is less than 50
guard attribute strength is greater than 80
```

**Flags:**
```
door is locked
player is confused
```

### Verb vs Trigger Tab

**Verb Condition** (uses Target1/Target2):
```
target state is open
target state is locked and target2 is not broken
```

**Trigger Condition** (global state):
```
player attribute health is less than 20
player is in kitchen
```

### Complex Examples Tab

**Multiple Conditions:**
```
player has key and door state is locked and player is in hallway
```

**OR Logic:**
```
player has key or player has lockpick
```

**Parenthesized Logic:**
```
(player has key or player has lockpick) and door state is locked
```

**Negation:**
```
not player is confused
door state is not locked
```

## Key Features

? **Natural Language Syntax** - All examples use human-readable format
? **Comparison Words** - Uses "is less than", "is greater than" instead of operators
? **No Underscores** - Shows natural space-separated words
? **Clear Explanations** - Each tab explains when to use that pattern
? **Real-World Examples** - Examples users would actually write
? **Progressive Complexity** - Starts simple, moves to complex
? **6 Focused Tabs** - Organized by concept and use case

## Comparison: Old vs New

### Old (Canonical DSL)
```
is_less_than
is_greater_than
player.attribute health is_less_than 50
```

### New (Natural Language)
```
is less than
is greater than
player attribute health is less than 50
```

## Tabs Organization

1. **Natural Language** - Prefix keywords and basic examples
2. **Implicit Subject** - How element names are inferred
3. **Operators** - Comparison and logical operators with examples
4. **Properties** - State, attributes, and flags access
5. **Verb vs Trigger** - Difference between two condition types
6. **Complex Examples** - Multi-condition logic and combinations

## User Benefits

? **Easier to Learn** - Natural language is more intuitive
? **Less Typing** - No underscores or special syntax
? **Better Discovery** - Examples show what's possible
? **Professional** - More polished and approachable
? **Self-Documenting** - Examples explain their own logic

## Implementation Notes

- All examples use proper natural language syntax
- Examples show what users type directly into conditions
- No transformation required - these are the actual input format
- Consistent with DSL canonicalizer behavior
- Examples match actual test data in test cases

## Testing

### Build Status ?
- Build succeeded with 0 errors
- No warnings

### Test Results ?
- Total: 135 tests
- Passed: 135
- Failed: 0
- Duration: ~500ms

## Example Workflow

**User sees in dialog:**
```
player health is less than 50
```

**User types it into condition tester:**
```
player health is less than 50
```

**Canonicalizer transforms it to:**
```
player.attribute health is_less_than 50
```

**Parser creates AST and evaluator runs it** ?

## Related Documentation

- See `NATURAL_LANGUAGE_DSL_ENHANCEMENT.md` for canonicalizer details
- See `CONDITION_TESTER_VERB_TRIGGER_ENHANCEMENT.md` for verb/trigger concepts
- See `UI_IMPROVEMENTS_EXAMPLES_AND_HOME.md` for dialog creation details

## Summary

? Updated all examples to natural language format
? 6 comprehensive tabs with clear organization
? Real-world examples users will actually use
? Better user experience and learning curve
? All tests passing (135/135)
? Professional, polished dialog

Users can now learn condition syntax from examples that match exactly what they'll type!
