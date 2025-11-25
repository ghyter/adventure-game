# Natural Language DSL - Quick Reference

## Effect Syntax

### State & Property Changes
```
set <target> to <semantic>              # Semantic resolution
set <target> <property> to <value>      # Explicit property
```

**Examples**:
```
set target to open
set player health to 10
set target visible to true
set target description to "An old door"
```

### Numeric Operations
```
increment <target> <key> by <amount>
decrement <target> <key> by <amount>
```

**Examples**:
```
increment player stamina by 1
decrement target battery by 5
increment player gold by 100
```

### Display & Output
```
describe <target>
show <target>
print "<message>"
```

**Examples**:
```
describe currentScene
show target
print "The door creaks open slowly."
```

### Special Actions
```
teleport <target>                  # Travel through exit
add <target> to inventory          # Pick up item
remove <target> from inventory     # Drop item
```

**Examples**:
```
teleport target
add target to inventory
remove torch from inventory
```

## Condition Syntax

### Type & State Checks
```
<subject> is <type>
<subject> is <state>
```

**Examples**:
```
target is exit
target is open
player is alive
```

### Inventory Checks
```
<subject> has <item>
```

**Examples**:
```
player has key
player has sword
target has torch
```

### Numeric Comparisons
```
<subject> <property> <operator> <value>
<subject> <property> is <value>
```

**Operators**: `>`, `<`, `>=`, `<=`, `==`, `!=`

**Examples**:
```
player health > 5
player gold >= 100
target durability < 10
player level is 5
```

### Logical Combinations
```
<condition> and <condition>
<condition> or <condition>
```

**Examples**:
```
player has key and target is locked
player health > 5 or player has potion
```

## Target Identifiers

| Identifier | Resolves To |
|------------|-------------|
| `player` | Current player |
| `currentPlayer` | Current player |
| `currentScene` | Current scene |
| `target` | First verb target |
| `target1` | First verb target |
| `target2` | Second verb target |
| `<name>` | Element by name or alias |

## Semantic Resolution Order

When using semantic names like "open", "visible", "health":

1. **States** - Named game states (highest priority)
2. **Flags** - Boolean flags
3. **Properties** - String properties
4. **Attributes** - Numeric attributes (lowest priority)

**Example**: 
- If element has both a state "open" and a flag "open"
- `set target to open` will set the **state** (higher priority)

## Value Types

| Type | Examples | Used For |
|------|----------|----------|
| String | `"text"`, `'text'` | Properties, messages |
| Integer | `10`, `-5`, `0` | Attributes, amounts |
| Boolean | `true`, `false` | Flags |
| Semantic | `open`, `closed`, `lit` | Auto-resolved |

## Common Patterns

### Open a Door
```
# Conditions
target is door
target is closed

# Effect
set target to open
```

### Pick Up an Item
```
# Conditions
target is item
player has space

# Effect
add target to inventory
```

### Travel Through Exit
```
# Conditions
target is exit
target is open

# Effect
teleport target
```

### Damage Player
```
# Effect
decrement player health by 10
```

### Heal Player
```
# Conditions
player health < 100

# Effect
increment player health by 20
```

### Check Quest Item
```
# Condition
player has quest_key and target is quest_door
```

### Multi-Step Effect
```
set target to unlocked
set target to open
print "The door swings open with a loud creak."
describe currentScene
```

## Error Messages

All effects return descriptive error messages:

```
"Error: Could not find element 'foo'"
"Error: door is not an exit"
"Error: Exit north has no paired exit"
"Error: No player to add item to"
"Error: Could not increment stamina on player"
```

## Tips & Best Practices

### 1. Use Semantic Names
? Good: `set target to open`
? Bad: `set target state to "open"`

### 2. Be Specific with Targets
? Good: `player has golden_key`
? Bad: `player has key` (if multiple keys exist)

### 3. Combine Conditions Logically
? Good: `player has key and target is locked`
? Good: `player health < 10 or player has potion`

### 4. Test with Tools
- Use **Effect Tester** to test effects
- Use **Condition Tester** to test conditions
- Iterate and refine

### 5. Provide Clear Messages
? Good: `print "The ancient door groans open, revealing a dark passage."`
? Bad: `print "Door open."`

## Integration with Verbs

```csharp
new Verb
{
    Name = "open",
    TargetCount = 1,
    ConditionTexts = new()
    {
        "target is door",
        "target is closed"
    },
    Effects = new()
    {
        new VerbEffect
        {
            Min = 0,
            Max = 0,  // Always succeeds
            SuccessText = "The door swings open.",
            FailureText = "",
            Action = "set target to open"  // Natural language!
        }
    }
}
```

## Multi-Line Effects

Separate each effect with a newline:

```
set target to open
print "The door opens."
describe currentScene
add key to inventory
```

Each line is parsed and executed in sequence.

## Cheat Sheet

| Want To... | Use... |
|------------|--------|
| Change state | `set target to <state>` |
| Check state | `target is <state>` |
| Change number | `increment/decrement <target> <key> by <n>` |
| Compare number | `<target> <key> > <value>` |
| Check type | `target is <type>` |
| Check inventory | `player has <item>` |
| Pick up | `add target to inventory` |
| Drop | `remove target from inventory` |
| Teleport | `teleport target` |
| Show description | `describe target` |
| Print text | `print "message"` |

## Supported Element Types

- `exit` - Exits between scenes
- `scene` - Locations
- `npc` - Non-player characters
- `item` - Pickupable objects
- `player` - Player character
- `level` - Level/floor

## Next Steps

1. Test effects in **Effect Tester** (`/tools/effects`)
2. Test conditions in **Condition Tester** (`/tools/conditions`)
3. Author verbs using natural language
4. Build complex interactions with combined conditions and effects

---

**Pro Tip**: Start simple with "describe" and "set to" effects, then progress to teleport and inventory as you get comfortable with the syntax!
