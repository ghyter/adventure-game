# Condition Tester Enhancements - Verb vs Trigger Conditions

## Overview

Enhanced the Condition & Effect Tester to support two distinct condition evaluation modes: **Verb Conditions** (with specific targets) and **Trigger Conditions** (global state evaluation).

## Features Implemented

### 1. ? Condition Type Selector
Added `RadzenDropDown` to switch between:
- **Verb Condition**: Evaluates with selected Target1 and optional Target2
- **Trigger Condition**: Evaluates without specific targets (global state)

**UI Location**: Top card in Condition Tester page

### 2. ? Target Selection Dropdowns (Verb Conditions Only)

**Target 1 Dropdown:**
```razor
<RadzenDropDown @bind-Value="@selectedTarget1Id" 
              Data="@availableElements" 
              TextProperty="Name"
              ValueProperty="Id"
              AllowClear="true"
              Placeholder="Select target element..."
              Style="width: 100%;"
              Change="@OnTarget1Changed" />
```

**Target 2 Dropdown:**
```razor
<RadzenDropDown @bind-Value="@selectedTarget2Id" 
              Data="@availableElements" 
              TextProperty="Name"
              ValueProperty="Id"
              AllowClear="true"
              Placeholder="Select second target..."
              Disabled="@(selectedTarget1Id == null)"  ? DISABLED UNTIL TARGET1 SET
              Style="width: 100%;" />
```

**Features:**
- ? Both use `ElementId` type for proper type safety
- ? Populated from `sandboxSession.Elements`
- ? Target2 disabled until Target1 is selected
- ? Both support clear/reset
- ? Show element names in dropdown

### 3. ? Enhanced GameSessionDslContext

Added properties for Target management:

```csharp
private class GameSessionDslContext : DslEvaluationContext
{
    public GameElement? Target { get; set; }
    public GameElement? Target1 { get; set; }
    public GameElement? Target2 { get; set; }

    public override object? GetTarget() => Target1 ?? Target;
    public override object? GetTarget2() => Target2;
}
```

**Null-safe evaluation**: All nullable properties properly handled

### 4. ? Condition Type Based Evaluation Logic

```csharp
if (conditionType == "Verb Condition")
{
    var target1 = FindElement(selectedTarget1Id);
    var target2 = FindElement(selectedTarget2Id);

    ctx = new GameSessionDslContext(sandboxSession)
    {
        Target = target1,
        Target1 = target1,
        Target2 = target2
    };
}
else // Trigger Condition
{
    ctx = new GameSessionDslContext(sandboxSession)
    {
        Target = null,
        Target1 = null,
        Target2 = null
    };
}
```

**Behavior:**
- **Verb Condition**: Passes selected targets to evaluation context
- **Trigger Condition**: Explicitly sets targets to null for global evaluation

### 5. ? Natural Language Preprocessor Integration

The `DslCanonicalizer` (acting as NLP preprocessor) is already integrated through `DslService`:

```csharp
// DslService internally calls canonicalizer
var result = dslService.ParseAndValidate(conditionText);
```

**Processing Pipeline:**
1. User enters natural language: `"when desk state is closed"`
2. `DslCanonicalizer.Canonicalize()` transforms it
3. Parser creates AST
4. Evaluator runs with proper context

### 6. ? UI/UX Improvements

**Conditional UI Display:**
```razor
@if (conditionType == "Verb Condition")
{
    <!-- Show Target1 and Target2 dropdowns -->
}
else
{
    <RadzenAlert>Trigger conditions evaluate without specific targets.</RadzenAlert>
}
```

**Updated Examples Dialog:**
- Added explanation of Verb vs Trigger conditions
- Clarified when to use each type
- Shows example conditions for both modes

## User Workflow

### Testing Verb Conditions

1. Select **"Verb Condition"** from dropdown
2. Choose **Target 1** element (e.g., "desk")
3. Optionally choose **Target 2** element (disabled until Target 1 set)
4. Add conditions that reference `target`: `"target.state is closed"`
5. Click **"Evaluate All"**
6. Context resolves `target` ? Target1 element

**Example:**
```
Condition Type: Verb Condition
Target 1: desk
Target 2: (none)
Condition: "when target state is closed"

Result: Evaluates desk.state against "closed"
```

### Testing Trigger Conditions

1. Select **"Trigger Condition"** from dropdown
2. Target dropdowns are hidden
3. Add conditions that reference global state: `"player.health is_less_than 50"`
4. Click **"Evaluate All"**
5. Context evaluates without specific targets

**Example:**
```
Condition Type: Trigger Condition
Condition: "when player health is_less_than 50"

Result: Evaluates player's global health attribute
```

## Technical Implementation Details

### Type Safety with ElementId

```csharp
// Properties use ElementId (not Ulid or string)
private ElementId? selectedTarget1Id = null;
private ElementId? selectedTarget2Id = null;

// Helper method handles ElementId comparison properly
private GameElement? FindElement(ElementId? elementId)
{
    if (elementId == null || sandboxSession == null) return null;
    return sandboxSession.Elements.FirstOrDefault(e => 
        e.Id.Value == elementId.Value.Value);
}
```

### State Management

```csharp
private void OnConditionTypeChanged()
{
    // Clear targets when switching to Trigger mode
    if (conditionType == "Trigger Condition")
    {
        selectedTarget1Id = null;
        selectedTarget2Id = null;
    }
}

private void OnTarget1Changed()
{
    // Clear Target2 when Target1 changes/clears
    if (selectedTarget1Id == null)
    {
        selectedTarget2Id = null;
    }
}
```

### Element Population

```csharp
private void InitializeSandbox()
{
    // ... existing code ...
    
    // Populate available elements for target dropdowns
    availableElements = sandboxSession.Elements.ToList();
}
```

## Testing

### All Tests Pass ?
- **Total**: 135 tests
- **Passed**: 135
- **Failed**: 0
- **Duration**: ~680ms

### Manual Testing Scenarios

**Scenario 1: Verb Condition with Target**
```
Type: Verb Condition
Target 1: "desk" (Item)
Condition: "target.state is closed"
Expected: Evaluates desk's state property
```

**Scenario 2: Verb Condition with Two Targets**
```
Type: Verb Condition  
Target 1: "key" (Item)
Target 2: "door" (Item)
Condition: "target is_in target2"
Expected: Checks if key is in door
```

**Scenario 3: Trigger Condition**
```
Type: Trigger Condition
Condition: "player.attribute health is_less_than 50"
Expected: Evaluates player's health globally
```

**Scenario 4: Target2 Disabled State**
```
Type: Verb Condition
Target 1: (none)
Expected: Target 2 dropdown is disabled ?
Action: Select Target 1
Expected: Target 2 dropdown becomes enabled ?
```

## Code Changes Summary

### Modified Files

1. **AdventureGame\Components\Pages\Tools\ConditionTester.razor**
   - Added condition type selector
   - Added Target1/Target2 dropdowns
   - Added conditional UI display logic
   - Enhanced GameSessionDslContext with Target properties
   - Implemented type-based evaluation logic
   - Added state management for targets

### Key Code Sections

**Condition Type Selector:**
```razor
<RadzenDropDown @bind-Value="@conditionType" 
              Data="@conditionTypes" 
              Change="@OnConditionTypeChanged" />
```

**Target Selection (Verb Mode):**
```razor
<RadzenFormField Text="Target 1">
    <RadzenDropDown @bind-Value="@selectedTarget1Id" ... />
</RadzenFormField>

<RadzenFormField Text="Target 2 (optional)">
    <RadzenDropDown @bind-Value="@selectedTarget2Id" 
                  Disabled="@(selectedTarget1Id == null)" ... />
</RadzenFormField>
```

**Evaluation Context Setup:**
```csharp
GameSessionDslContext ctx;

if (conditionType == "Verb Condition")
{
    ctx = new GameSessionDslContext(sandboxSession)
    {
        Target = FindElement(selectedTarget1Id),
        Target1 = FindElement(selectedTarget1Id),
        Target2 = FindElement(selectedTarget2Id)
    };
}
else
{
    ctx = new GameSessionDslContext(sandboxSession)
    {
        Target = null,
        Target1 = null,
        Target2 = null
    };
}
```

## Benefits

1. **? Clear Distinction**: Users understand Verb vs Trigger conditions
2. **? Proper Target Management**: Target1/Target2 properly passed to evaluator
3. **? UI Guidance**: Conditional display guides users
4. **? Type Safety**: ElementId used throughout for type safety
5. **? Disabled State**: Target2 appropriately disabled until Target1 set
6. **? NLP Integration**: Natural language already working via DslCanonicalizer
7. **? Null Safety**: All nullable references handled properly
8. **? State Reset**: Switching modes clears inappropriate state

## Example Use Cases

### Verb Condition: "Use Key on Door"
```
Type: Verb Condition
Target 1: golden_key (Item)
Target 2: main_door (Item)
Conditions:
  - "target.state is available"  // key is available
  - "target2.state is locked"    // door is locked
  - "player has target"          // player has the key

Result: All must be true to use key on door
```

### Trigger Condition: "Player Low Health"
```
Type: Trigger Condition
Conditions:
  - "player.attribute health is_less_than 20"
  - "player is_in combat_area"

Result: Triggers when player is low health in combat
```

## Future Enhancements

### Potential Additions
- **Save/Load Target Configurations**: Remember last used targets
- **Target Validation**: Warn if targets incompatible with condition
- **Multi-Select**: Support selecting multiple targets for complex conditions
- **Visual Target Indicators**: Highlight selected targets in SessionAudit
- **Condition Templates**: Pre-built condition sets for common verb scenarios

### Integration Points
- **Verb Editor**: Pre-populate targets from verb definition
- **Trigger Editor**: Pre-populate conditions from trigger definition
- **Test Library**: Save frequently tested condition+target combinations

## Summary

Successfully implemented all requested features:

- ? Condition type selector (Verb vs Trigger)
- ? Target1 dropdown with element selection
- ? Target2 dropdown (disabled until Target1 set)
- ? Elements from session.Elements
- ? NLP pre-parser integration (via DslCanonicalizer)
- ? Enhanced GameSessionDslContext with Target1/Target2
- ? Null-safe evaluation
- ? Type-based evaluation logic
- ? Proper state management

**All tests passing**: 135/135 ?

The Condition Tester now properly supports both Verb Conditions (with specific targets) and Trigger Conditions (global state), providing a comprehensive testing environment for both use cases.
