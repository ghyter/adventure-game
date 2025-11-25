# User Guide: Enhanced Verb System

## Quick Start

### 1. Creating a Zero-Target Verb

Verbs like "look", "wait", "help" don't require targets.

**In VerbEditor:**
1. Enter Name: "look"
2. Set Target Count: **0**
3. Target sections automatically hide
4. Add conditions and effects as needed
5. Save

**Usage:**
```
Player: > look
System: [Displays scene description]
```

### 2. Creating a One-Target Verb

Verbs like "examine", "take", "open" require one target.

**In VerbEditor:**
1. Enter Name: "examine"
2. Set Target Count: **1**
3. Target 1 section appears
4. Configure Target 1 filter:
   - Mode: All (accepts any element)
   - Or: Types (e.g., "item", "npc")
   - Or: Tags (e.g., "drawable", "interactive")
   - Or: Names (specific element names)
5. Target 2 is hidden
6. Add conditions and effects
7. Save

**Usage:**
```
Player: > examine door
System: [Matches verb with Target1=door]

Player: > take apple
System: [Matches verb with Target1=apple]

Player: > look book
System: [Does NOT match "look" (0-target)]
       [Does match "examine" (1-target)]
```

### 3. Creating a Two-Target Verb

Verbs like "use ... on ...", "give ... to ...", "put ... in ...".

**In VerbEditor:**
1. Enter Name: "use"
2. Set Target Count: **2**
3. Both Target 1 and Target 2 sections appear
4. Configure Target 1:
   - Tags: "tool", "weapon"
5. Configure Target 2:
   - Tags: "interactive", "container"
6. Add conditions (e.g., "when player has target1")
7. Add effects
8. Save

**Usage:**
```
Player: > use key door
System: [Matches verb with Target1=key, Target2=door]

Player: > use knife rope
System: [Matches verb with Target1=knife, Target2=rope]

Player: > use key
System: [Does NOT match "use" (2-target required)]
       [May match other 1-target verbs instead]
```

## Testing Verbs

### Via Verb Tester (`/tools/verbs`)

1. **Load a GamePack** with verbs defined
2. **Navigate to**: Tools > Verb Tester
3. **Enter a command**: "examine door"
4. **See results**:
   - Matched verb: "examine"
   - Target 1: "door" element
   - Target score: Why this verb matched

### Via Effect Tester (`/tools/effects`)

1. **Navigate to**: Tools > Effect Tester
2. **Set Execution Context**:
   - Current Player: Select a player
   - Target 1: Select a target (if applicable)
   - Target 2: Select a second target (if applicable)
   - Current Scene: Select the scene
3. **Define an effect**:
   - Min Roll: 10
   - Max Roll: 15
   - Success Message: "The door opens successfully."
   - Failure Message: "The door won't budge."
4. **Click Execute Effect**
5. **See Results**: Success/Failure with roll information

## Testing Conditions

### Via Condition Tester (`/tools/conditions`)

1. **Navigate to**: Tools > Condition & Effect Tester
2. **Set Context**:
   - Condition Type: "Verb Condition" or "Trigger Condition"
   - Player: Select a player
   - Target 1: (for Verb Condition)
   - Target 2: (for Verb Condition)
   - Current Scene: Select the scene
3. **Add Conditions**:
   - Enter DSL text: "when player has key"
   - Or: "when door state is locked"
   - Or: "when target is in player"
4. **See Results**:
   - ? Condition passes (green)
   - ? Condition fails (red)
   - Canonical form of the condition
   - Equivalent C# expression

## Understanding Target Scoring

### Mode Scoring (Higher = More Specific)

| Mode | Score | Use Case |
|------|-------|----------|
| None | 0 | Target not used |
| All | 10 | Any element matches |
| Types | 50 | Match by element type (item, npc, etc.) |
| Tags | 80 | Match by tag (tool, weapon, etc.) |
| Names | 100 | Match specific names (exact match wins) |

### Example: Disambiguating "use"

Given a scene with:
- brass key (tags: "key", "tool")
- golden key (tags: "key", "tool")
- door (tags: "interactive")

**Verb definitions:**

```
Verb: use
- Target1: Mode=Names, Names=["brass key"]
- Target2: Mode=All

Verb: use
- Target1: Mode=Tags, Tags=["key"]
- Target2: Mode=All

Verb: use
- Target1: Mode=All
- Target2: Mode=All
```

**When player types: "use brass key door"**

1. All three verbs match (both targets present)
2. First verb scores 100 (name match) + 10 (all) = 110 ? **WINNER**
3. Second verb scores 80 (tag match) + 10 (all) = 90
4. Third verb scores 10 (all) + 10 (all) = 20

? **Result**: Most specific match wins!

## Common Patterns

### Interactive Elements
```csharp
// For elements players can interact with
Target1 = new()
{
    Mode = GameElementFilterMode.Tags,
    Tags = new() { "interactive" }
}
```

### Tools/Weapons
```csharp
Target1 = new()
{
    Mode = GameElementFilterMode.Tags,
    Tags = new() { "tool", "weapon" }
}
```

### Containers
```csharp
Target2 = new()
{
    Mode = GameElementFilterMode.Tags,
    Tags = new() { "container" }
}
```

### NPCs Only
```csharp
Target1 = new()
{
    Mode = GameElementFilterMode.Types,
    Types = new() { "npc" }
}
```

### Specific Items
```csharp
Target1 = new()
{
    Mode = GameElementFilterMode.Names,
    Names = new() { "golden key", "brass key" }
}
```

## Effect Definition

### Always Succeeds
```csharp
effect.Min = 0;
effect.Max = 0;
effect.SuccessText = "You successfully take the {target}.";
effect.FailureText = null;
```

### Requires Roll in Range
```csharp
effect.Min = 5;
effect.Max = 15;
effect.SuccessText = "The door swings open!";
effect.FailureText = "The door won't budge.";
```

### Skill-Based (Difficulty 10)
```csharp
// Player rolls 1d20
// 1 = always fail
// 20 = always succeed
// 10-15 = in range, success
effect.Min = 10;
effect.Max = 15;
```

### Coin Flip (50/50)
```csharp
effect.Min = 10;
effect.Max = 10;
// Only exactly 10 succeeds (5% chance, closest to 50%)
```

## Troubleshooting

### Verb Not Matching

**Issue**: Player types "use key door" but verb isn't selected.

**Diagnose**:
1. Go to Verb Tester
2. Type the exact command: "use key door"
3. Check if verb appears
4. If not, check:
   - TargetCount matches (2-target verb needed)
   - Target1 filter includes "key"
   - Target2 filter includes "door"

**Fix**:
- Open VerbEditor
- Check Target Count is 2
- Check Target1 filter mode and values
- Check Target2 filter mode and values

### Condition Not Evaluating

**Issue**: Condition always fails even when it should pass.

**Diagnose**:
1. Go to Condition Tester
2. Type the condition: "when player has key"
3. Set the Player and Scene
4. Check if it parses (no errors)
5. Check if it evaluates to true/false

**Debug Steps**:
- Look at the canonical form: Does it make sense?
- Look at the C# expression: Is the syntax right?
- Check Session Audit: Does the player actually have the item?

### Effect Always Fails

**Issue**: Effect always shows failure message even with good rolls.

**Diagnose**:
1. Go to Effect Tester
2. Set Min and Max
3. Click Execute Effect multiple times
4. Watch the roll results

**Potential Issues**:
- Min/Max range too narrow
- Min > Max (invalid range)
- Item/target not found in context

## Best Practices

### 1. Tag Your Elements
- Give items meaningful tags: "tool", "weapon", "key", "container"
- Give NPCs meaningful tags: "shopkeeper", "guard", "merchant"
- Use tags to create semantic verb categories

### 2. Use Specific Names Sparingly
- Names (100 score) beat everything
- Reserve for truly unique items
- Use Tags (80 score) for categories

### 3. Group Related Verbs
```
- use: "use {tool} {target}"  [2-target]
- use: "use {key}" [1-target, assumes door in room]
- activate: "activate {interactive}" [1-target]
```

### 4. Test Early, Test Often
- Use Condition Tester while writing conditions
- Use Effect Tester for each effect scenario
- Use Verb Tester with actual game commands

### 5. Conditions Before Effects
- Make sure all conditions are correct first
- Then test effects
- Then test full verb flow

## Command Resolution Flow

```
Player Input: "use key door"
        ?
Parser: Extract verb="use", targets=["key", "door"]
        ?
Find Elements: key=<Item>, door=<Item>
        ?
VerbResolver: 
  - Count targets: 2
  - Find verbs with TargetCount=2
  - Score each by filter match
  - Return highest scoring verb
        ?
Verb Found: "use" with effects=[...]
        ?
Check Conditions:
  - ALL must pass or no effect executes
        ?
Execute Effects:
  - Roll if needed
  - Show success/failure text
  - Apply actions
        ?
Update State:
  - Move items
  - Change states
  - Update session
        ?
Show Output:
  - Success/failure message
  - Any status changes
```

## Examples

### Simple Verb: Look
```csharp
new Verb
{
    Name = "look",
    Aliases = new() { "watch" },
    TargetCount = 0,
    ConditionTexts = new(),
    Effects = new()
    {
        new VerbEffect
        {
            Min = 0,
            Max = 0,
            SuccessText = "You see {currentScene.Name}: {currentScene.Description}",
            FailureText = null,
            Action = "DisplayScene"
        }
    }
}
```

### One-Target Verb: Examine
```csharp
new Verb
{
    Name = "examine",
    Aliases = new() { "look at", "inspect" },
    TargetCount = 1,
    Target1 = new()
    {
        Mode = GameElementFilterMode.All
    },
    ConditionTexts = new()
    {
        "when target exists",
        "when player can see target"
    },
    Effects = new()
    {
        new VerbEffect
        {
            Min = 0,
            Max = 0,
            SuccessText = "You examine the {target.Name}: {target.Description}",
            FailureText = "You can't examine that.",
            Action = "Examine:target"
        }
    }
}
```

### Two-Target Verb: Use Key on Door
```csharp
new Verb
{
    Name = "use",
    Aliases = new() { "apply", "try" },
    TargetCount = 2,
    Target1 = new()
    {
        Mode = GameElementFilterMode.Tags,
        Tags = new() { "key", "tool" }
    },
    Target2 = new()
    {
        Mode = GameElementFilterMode.Tags,
        Tags = new() { "interactive", "lock" }
    },
    ConditionTexts = new()
    {
        "when player has target1",
        "when target2.state is locked"
    },
    Effects = new()
    {
        new VerbEffect
        {
            Min = 10,
            Max = 15,
            SuccessText = "Using the {target1.Name}, you unlock the {target2.Name}.",
            FailureText = "The {target1.Name} doesn't fit the {target2.Name}.",
            Action = "Unlock:target2"
        }
    }
}
```

## Next Steps

1. **Create Your Verbs**: Start with simple 0-target verbs, progress to complex ones
2. **Define Conditions**: Use Condition Tester to validate
3. **Test Effects**: Use Effect Tester with various rolls
4. **Play Test**: Use actual gameplay to discover issues
5. **Refine**: Adjust filters, conditions, and effects based on testing

Happy verb authoring! ??
