# Verbs, Filters, Triggers, Command Parser, Verb Tester, and Editor Implementation

## Overview

This implementation provides a complete verb and trigger system for the Adventure Game engine, including:

- **GameElementFilter**: Reusable filtering system for game elements
- **Verb**: Actions that players can perform with target filtering
- **VerbEffect**: Effects that can be applied with difficulty checks
- **Trigger**: Automatic events that fire when conditions are met
- **VerbResolver**: Matches player input to available verbs based on targets
- **CommandParser**: Parses natural language commands into structured format
- **ActionExecutor**: Executes game actions like "describe"
- **VerbEditor**: Blazor component for editing verbs
- **VerbTester**: Testing tool for verbs and command parsing

## Architecture

### Core Models

#### GameElementFilter (`AdventureGame.Engine/Filters/GameElementFilter.cs`)

Provides flexible filtering of game elements with multiple modes:

- **All**: Matches all elements (score: 10)
- **Types**: Matches by element kind (item, npc, scene, etc.) (score: 50)
- **Tags**: Matches by element tags (score: 80)  
- **Names**: Matches by name or alias (score: 100)

Higher scores indicate more specific matches, allowing the VerbResolver to prefer more specific verbs.

#### Verb (`AdventureGame.Engine/Verbs/Verb.cs`)

Represents a player action with:

- **Name**: Canonical verb name (e.g., "take", "use")
- **Aliases**: Alternative names (e.g., "grab", "pick up")
- **Tags**: Categorization tags
- **Target1/Target2**: GameElementFilter for each target
- **ConditionTexts**: List of DSL condition strings that must be met
- **Effects**: List of VerbEffect to apply when executed

#### VerbEffect (`AdventureGame.Engine/Verbs/VerbEffect.cs`)

Defines outcomes of verb execution:

- **Min/Max**: Difficulty check range
  - 0/0 = always succeeds
  - Otherwise roll d20, with 1=auto-fail, 20=auto-success
- **SuccessText/FailureText**: Messages to display
- **Action**: Action string to execute (e.g., "ChangeState:door:open")

#### Trigger (`AdventureGame.Engine/Triggers/Trigger.cs`)

Automatic events that fire when conditions are met:

- **Name**: Trigger identifier
- **ConditionTexts**: DSL conditions that must be met
- **Effects**: VerbEffects to apply when triggered
- **FiredThisRound**: Prevents multiple firings per round

Triggers are evaluated after each round. A trigger fires at most once per round.

### Execution Flow

#### VerbResolver (`AdventureGame.Engine/Verbs/VerbResolver.cs`)

Matches player input to the best verb:

1. Find verbs matching the verb token (by name or alias)
2. Score each candidate based on target filters
3. Return the highest-scoring verb

Example:
```
Input: "use key door"
Candidates:
  - "use" with Target1=All, Target2=All (score: 20)
  - "use" with Target1=Names["key"], Target2=Names["door"] (score: 200)
Selected: Second verb (higher score)
```

#### CommandParser (`AdventureGame.Engine/Parser/CommandParser.cs`)

Parses natural language input:

1. Remove fluff words ("the", "a", "to", etc.)
2. First token = verb
3. Subsequent tokens = targets
4. Resolve directional words to Exit elements
5. Resolve names/aliases to GameElements

Supported formats:
- `verb` (e.g., "look")
- `verb target` (e.g., "take apple")
- `verb target1 target2` (e.g., "use key door")
- `verb direction` (e.g., "go north")

#### ActionExecutor (`AdventureGame.Engine/Actions/DescribeAction.cs`)

Executes built-in actions:

- **describe**: Returns formatted description of element including current state

## UI Components

### GameElementFilterControl (`AdventureGame/Components/Shared/GameElementFilterControl.razor`)

Reusable component for configuring GameElementFilter:

- Mode dropdown (All/Types/Tags/Names)
- Dynamic multi-select based on mode
- Auto-populates available types, tags, and names from elements
- Syncs changes back to Filter model

### VerbEditor (`AdventureGame/Components/Pages/Tools/VerbEditor.razor`)

Full-featured verb editor:

- Name, aliases, tags editing
- Target1/Target2 filter configuration
- Conditions list with add/remove
- Effects editor with Min/Max, messages, and actions
- Integrates with CurrentGameService for dirty tracking

### VerbTester (`AdventureGame/Components/Pages/Tools/VerbTester.razor`)

Interactive testing tool:

- Command input with Enter key support
- Parse results display (verb token, targets)
- Resolved verb display
- Condition evaluation results
- Effect outcome simulation
- Session audit integration

## Usage Examples

### Creating a "Take" Verb

```csharp
var takeVerb = new Verb
{
    Name = "take",
    Aliases = new List<string> { "grab", "pick up", "get" },
    Target1 = new GameElementFilter 
    { 
        Mode = GameElementFilterMode.Types,
        Types = new List<string> { "item" }
    },
    ConditionTexts = new List<string>
    {
        "when target is visible",
        "when player location is target location"
    },
    Effects = new List<VerbEffect>
    {
        new VerbEffect
        {
            Min = 0,
            Max = 0, // Always succeeds
            SuccessText = "You take the {target}.",
            Action = "MoveTo:target:player"
        }
    }
};
```

### Creating a "Use Key on Door" Verb

```csharp
var useKeyDoorVerb = new Verb
{
    Name = "use",
    Aliases = new List<string> { "unlock" },
    Target1 = new GameElementFilter
    {
        Mode = GameElementFilterMode.Names,
        Names = new List<string> { "key" }
    },
    Target2 = new GameElementFilter
    {
        Mode = GameElementFilterMode.Names,
        Names = new List<string> { "door" }
    },
    ConditionTexts = new List<string>
    {
        "when player has key",
        "when door state is locked"
    },
    Effects = new List<VerbEffect>
    {
        new VerbEffect
        {
            Min = 10,
            Max = 20, // Difficulty 10-20
            SuccessText = "You unlock the door with the key.",
            FailureText = "The key doesn't seem to fit.",
            Action = "ChangeState:door:unlocked"
        }
    }
};
```

### Creating a Trigger

```csharp
var enterDungeonTrigger = new Trigger
{
    Name = "DungeonEntrance",
    ConditionTexts = new List<string>
    {
        "when player location is dungeon_entrance",
        "when player has torch"
    },
    Effects = new List<VerbEffect>
    {
        new VerbEffect
        {
            Min = 0,
            Max = 0,
            SuccessText = "The torch illuminates ancient runes on the walls.",
            Action = "ChangeState:runes:visible"
        }
    }
};
```

## Testing

### Using the Verb Tester

1. Navigate to **Tools > Verb Tester**
2. Enter a command (e.g., "take apple")
3. View parse results showing verb token and resolved targets
4. See which verb was selected and why
5. Check condition evaluation results
6. Review effect outcomes

### Command Parser Examples

```
Input: "take the apple"
? Verb: "take", Target1: apple (item), Target2: null

Input: "use key on door"
? Verb: "use", Target1: key (item), Target2: door (item)

Input: "go north"
? Verb: "go", Target1: north_exit (exit), Target2: null

Input: "examine guard"
? Verb: "examine", Target1: guard (npc), Target2: null
```

## Integration Points

### GamePack Model

The `GamePack` model already includes:
- `List<Verb> Verbs` (different namespace - `AdventureGame.Engine.Models.Round.Verb`)
- `List<GameTrigger> Triggers`

**Note**: The implementation creates new models in separate namespaces:
- `AdventureGame.Engine.Verbs.Verb` (new simplified model)
- `AdventureGame.Engine.Triggers.Trigger` (new simplified model)

These are complementary systems. The existing Round.Verb is more complex with structured authoring. The new simplified models can coexist for different use cases.

### DSL Integration

Condition evaluation uses the existing DSL system:
- Parse condition texts using `DslService.ParseAndValidate()`
- Evaluate using `DslService.Evaluate(ast, context)`
- Context includes player, targets, current scene, session state

### Session State

The VerbTester creates a sandbox session for testing:
- Uses `IGameSessionFactory.CreateSandboxSession()`
- Independent from main game session
- Safe for experimentation without affecting saved state

## File Structure

```
AdventureGame.Engine/
??? Filters/
?   ??? GameElementFilter.cs
??? Verbs/
?   ??? Verb.cs
?   ??? VerbEffect.cs
?   ??? VerbResolver.cs
??? Triggers/
?   ??? Trigger.cs
??? Parser/
?   ??? CommandParser.cs
??? Actions/
    ??? DescribeAction.cs

AdventureGame/Components/
??? Shared/
?   ??? GameElementFilterControl.razor
??? Pages/Tools/
    ??? VerbEditor.razor
    ??? VerbTester.razor
```

## Future Enhancements

1. **Verb Execution Engine**: Apply effects to session state
2. **Trigger Evaluation Loop**: Auto-fire triggers after each round
3. **Scope Resolution**: Dynamic scope for effect targets (current scene, visible items, etc.)
4. **Action Types**: Define standard action types (MoveTo, ChangeState, SetAttribute, etc.)
5. **Difficulty Modifiers**: Apply modifiers from attributes, equipment, etc.
6. **Effect Ranges**: Multiple overlapping effect ranges based on roll
7. **Condition Editor**: Visual condition builder instead of text input
8. **Verb Chaining**: Allow verbs to trigger other verbs

## Implementation Status

? **Completed**:
- GameElementFilter with scoring
- Verb model with targets and effects
- VerbEffect with difficulty checks
- Trigger model
- VerbResolver with scoring algorithm
- CommandParser with direction resolution
- DescribeAction executor
- GameElementFilterControl component
- VerbEditor page
- VerbTester page
- Documentation

? **Pending**:
- Integration with existing GamePack.Verbs (different model)
- Actual effect execution on session state
- Trigger evaluation loop
- Condition evaluation in VerbTester
- Navigation menu updates

## Related Documentation

- Natural Language DSL Enhancement
- Condition Tester Implementation  
- Session Audit Hierarchy
- Game Element Tags and Elements Page
